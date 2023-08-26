using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture.Xunit2;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sample.BillingAccount.Api.Configuration;
using Sample.BillingAccount.Api.Constants;
using Sample.BillingAccount.Api.Logging;
using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Internal;
using Sample.BillingAccount.Api.Repositories;
using Sample.BillingAccount.Api.Serialization;
using Sample.BillingAccount.Api.TestUtils;
using Sample.BillingAccount.Api.TestUtils.AutoData;
using Sample.BillingAccount.Api.TestUtils.Extensions;
using Sample.BillingAccount.Api.TestUtils.Helpers;
using Xunit;

namespace Sample.BillingAccount.Api.UnitTests.Repositories;

public class CreditAccountRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task SaveBillingAccounts_ShouldUploadFilesToS3_WithExpectedFilePathAndContent(
        List<CustomerAccount> creditAccount,
        [Frozen] IOptions<S3Settings> s3Settings,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        string customerId,
        [Frozen] ITestLoggerFactory testLoggerFactory,
        BillingAccountRepository sut
    )
    {
        // Arrange
        var key = CreateS3FileKey(s3Settings.Value, customerId);

        var putObjectRequests = new List<PutObjectRequest>();
        amazonS3Mock
            .Setup(p => p.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK })
            .Callback((PutObjectRequest request, CancellationToken _) =>
            {
                putObjectRequests.Add(request);
            });

        // Act
        await sut.SaveBillingAccounts(creditAccount);

        // Assert
        amazonS3Mock.Verify(p => p.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Exactly(3));
        amazonS3Mock.VerifyNoOtherCalls();

        for (var i = 0; i < putObjectRequests.Count; i++)
        {
            putObjectRequests[i].BucketName.Should().Be(s3Settings.Value.BucketName);
            putObjectRequests[i].ContentType.Should().Be(MediaTypeNames.Application.Json);
            putObjectRequests[i].ContentBody.Should().Be(JsonSerializer.Serialize(creditAccount[i], SerializerOptions.Default));
            putObjectRequests[i].Key.Should().Contain(creditAccount[i].CustomerId);

            testLoggerFactory.VerifyMessageLogged(
                LogLevel.Information,
                expectedEventId: LogEvents.StoredEntity,
                expectedEventName: nameof(LogEvents.StoredEntity),
                expectedMessage: LogMessages.StoredEntity.ToStructuredLogMessage(HttpStatusCode.OK.ToString(), s3Settings.Value.BucketName, CreateS3FileKey(s3Settings.Value, creditAccount[i].CustomerId)),
                times: Times.Exactly(1));
        }
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAccountAndAddTransaction_WhenCustomerFilesExistsInS3_ThenShouldDownloadFilesAndAddTransactionToIt(
        List<CustomerAccount> creditAccount,
        [Frozen] IOptions<S3Settings> s3Settings,
        TransactionsDto transactionDto,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        BillingAccountRepository sut
    )
    {
        // Arrange
        for (var i = 0; i < creditAccount.Count; i++)
        {
            var charge = transactionDto.Transactions[i].Value.Amount;
            creditAccount[i].CustomerId = transactionDto.Transactions[i].CustomerId;
            var key = CreateS3FileKey(s3Settings.Value, creditAccount[i].CustomerId);

            amazonS3Mock
                .Setup(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default))
                .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = StreamHelper.GenerateStreamFromString(JsonSerializer.Serialize(creditAccount[i], SerializerOptions.Default)) });
            creditAccount[i].CreditAccount.Balance.Amount -= charge;
            creditAccount[i].CreditAccount.Statements.Add(AddStatement(transactionDto.Transactions[i], creditAccount[i].CreditAccount.Balance.Amount));
        }

        // Act
        var result = await sut.GetAccountAndAddTransaction(transactionDto);

        // Assert
        for (var i = 0; i < creditAccount.Count; i++)
        {
            amazonS3Mock.Verify(p => p.GetObjectAsync(s3Settings.Value.BucketName, CreateS3FileKey(s3Settings.Value, creditAccount[i].CustomerId), default), Times.Once);
        }
        amazonS3Mock.VerifyNoOtherCalls();

        result.Should().BeEquivalentTo(creditAccount, options =>
        {
            options.AllowingInfiniteRecursion();
            return options;
        });
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAccountAndAddTransaction_WhenCustomerFilesDoesNotExistInS3_ThenAddANewFileContent(
        List<CustomerAccount> creditAccount,
        [Frozen] IOptions<S3Settings> s3Settings,
        TransactionsDto transactionDto,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        BillingAccountRepository sut
    )
    {
        // Arrange
        creditAccount.RemoveAll(_ => true);

        foreach (var transaction in transactionDto.Transactions)
        {
            var key = CreateS3FileKey(s3Settings.Value, transaction.CustomerId);

            amazonS3Mock
                .Setup(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default))
                .ReturnsAsync((new GetObjectResponse { HttpStatusCode = HttpStatusCode.NotFound }));

            creditAccount.Add(AddNewAccount(transaction));
        }

        // Act
        var result = await sut.GetAccountAndAddTransaction(transactionDto);

        // Assert
        for (var i = 0; i < creditAccount.Count; i++)
        {
            amazonS3Mock.Verify(p => p.GetObjectAsync(s3Settings.Value.BucketName, CreateS3FileKey(s3Settings.Value, creditAccount[i].CustomerId), default), Times.Once);
        }
        amazonS3Mock.VerifyNoOtherCalls();

        result.Should().BeEquivalentTo(creditAccount, options =>
        {
            options.AllowingInfiniteRecursion();
            options.Excluding(x => x.Path.Contains("Amount"));
            return options;
        });
    }

    [Theory]
    [AutoMoqData]
    public void GetAccountAndAddTransaction_WhenS3ClientThrowsException_ShouldLogAndThrowException(
        TransactionsDto transactionDto,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        string exceptionMessage,
        BillingAccountRepository sut
    )
    {
        // Arrange
        amazonS3Mock
        .Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Throws(new AmazonS3Exception(exceptionMessage));

        // Act & Assert
        FluentActions.Invoking(() => sut.GetAccountAndAddTransaction(transactionDto))
            .Should().ThrowAsync<AmazonS3Exception>()
            .WithMessage(exceptionMessage);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetCustomerAccount_WhenCustomerFileExistInS3_ThenReturnsCustomerAccount(
        [Frozen] IOptions<S3Settings> s3Settings,
        string customerId,
        CustomerAccount customerAccount,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        BillingAccountRepository sut
    )
    {
        // Arrange
        var key = CreateS3FileKey(s3Settings.Value, customerId);
        amazonS3Mock
            .Setup(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default))
            .ReturnsAsync((new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = StreamHelper.GenerateStreamFromString(JsonSerializer.Serialize(customerAccount, SerializerOptions.Default)) }));

        // Act
        var result = await sut.GetCustomerAccount(customerId);

        // Assert

        amazonS3Mock.Verify(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default), Times.Once);
        amazonS3Mock.VerifyNoOtherCalls();

        result.Should().BeEquivalentTo(customerAccount);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetCustomerAccount_WhenCustomerFilesDoesNotExistInS3_ThenReturnsEmptyObject(
        [Frozen] IOptions<S3Settings> s3Settings,
        string customerId,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        BillingAccountRepository sut
    )
    {
        // Arrange
        var key = CreateS3FileKey(s3Settings.Value, customerId);
        amazonS3Mock
            .Setup(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default))
            .ReturnsAsync((new GetObjectResponse { HttpStatusCode = HttpStatusCode.NotFound }));

        // Act
        var result = await sut.GetCustomerAccount(customerId);

        // Assert
        amazonS3Mock.Verify(p => p.GetObjectAsync(s3Settings.Value.BucketName, key, default), Times.Once);
        amazonS3Mock.VerifyNoOtherCalls();

        result.CustomerId.Should().BeEmpty();
    }

    [Theory]
    [AutoMoqData]
    public void GetCustomerAccount_WhenS3ClientThrowsException_ShouldLogAndThrowException(
        string customerId,
        [Frozen] Mock<IAmazonS3> amazonS3Mock,
        string exceptionMessage,
        BillingAccountRepository sut
    )
    {
        // Arrange
        amazonS3Mock
        .Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Throws(new AmazonS3Exception(exceptionMessage));

        // Act & Assert
        FluentActions.Invoking(() => sut.GetCustomerAccount(customerId))
            .Should().ThrowAsync<AmazonS3Exception>()
            .WithMessage(exceptionMessage);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAccountAndAddTransaction_WhenNewTransactionAdded_BalanceShouldBeUpdated(
      Transaction transaction,
      [Frozen] Mock<IAmazonS3> amazonS3Mock,
      CustomerAccount customerAccount,
      BillingAccountRepository sut
    )
    {
        // Arrange
        var transactionDto = new TransactionsDto();
        transactionDto.Transactions.Add(transaction);
        var startingBalance = customerAccount.CreditAccount.Balance.Amount;
        var charge = transaction.Value.Amount;
        amazonS3Mock
        .Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = StreamHelper.GenerateStreamFromString(JsonSerializer.Serialize(customerAccount, SerializerOptions.Default)) });

        //  Act
        var result = await sut.GetAccountAndAddTransaction(transactionDto);

        //  Assert
        result.ForEach(x => x.CreditAccount.Balance.Amount.Should().Be(startingBalance - charge));
    }

    private CustomerAccount AddNewAccount(Transaction transaction)
    {
        return new CustomerAccount
        {
            CustomerId = transaction.CustomerId,
            CreditAccount = new CreditAccount
            {
                Balance = new Price { Amount = 0, CurrencyCode = CurrencyCodes.Gbp },
                Statements = new List<Statement> { AddStatement(transaction, 0) }
            }
        };
    }

    private Statement AddStatement(Transaction transaction, decimal balance)
    {
        var statement = new Statement
        {
            Balance = new Price { Amount = balance, CurrencyCode = CurrencyCodes.Gbp },
            Date = transaction.Date
        };

        //Charge
        statement.Transactions.Add(new() { Type = transaction.Type, Value = transaction.Value });
        //Payment
        statement.Transactions.Add(new() { Type = Model.TransactionType.Payment, Value = transaction.Value });

        return statement;
    }

    private string CreateS3FileKey(S3Settings s3Settings, string customerId)
    {
        return $"{s3Settings.Folder}/{s3Settings.FileName}_{customerId}{s3Settings.FileExtension}";
    }
}
