using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Sample.BillingAccount.Api.Configuration;
using Sample.BillingAccount.Api.Constants;
using Sample.BillingAccount.Api.Logging;
using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Internal;
using Sample.BillingAccount.Api.Serialization;

namespace Sample.BillingAccount.Api.Repositories;

public interface IBillingAccountRepository
{
    Task SaveBillingAccounts(List<CustomerAccount> customerBillingAccounts);
    Task<List<CustomerAccount>> GetAccountAndAddTransaction(TransactionsDto transactionsDto);
    Task<CustomerAccount> GetCustomerAccount(string customerId);
}

public class BillingAccountRepository : IBillingAccountRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<BillingAccountRepository> _logger;
    private readonly IOptions<S3Settings> _s3Settings;
    private const decimal StartingBalance = 100;

    public BillingAccountRepository(IAmazonS3 s3Client, ILogger<BillingAccountRepository> logger, IOptions<S3Settings> s3Settings)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Settings = s3Settings;
    }

    public async Task SaveBillingAccounts(List<CustomerAccount> customerBillingAccounts)
    {
        try
        {
            foreach (var customerAccount in customerBillingAccounts)
            {
                var value = JsonSerializer.Serialize(customerAccount, SerializerOptions.Default);
                var key = CreateS3FileKey(customerAccount.CustomerId);

                _logger.UploadingEntity(_s3Settings.Value.BucketName, key);

                var request = new PutObjectRequest
                {
                    BucketName = _s3Settings.Value.BucketName,
                    Key = key,
                    ContentType = MediaTypeNames.Application.Json,
                    ContentBody = value
                };

                var response = await _s3Client.PutObjectAsync(request);

                _logger.StoredEntity(response.HttpStatusCode.ToString(), _s3Settings.Value.BucketName, key);
            }
        }
        catch (Exception e)
        {
            _logger.FailedToStoreEntity(e);
            throw;
        }
    }

    public async Task<List<CustomerAccount>> GetAccountAndAddTransaction(TransactionsDto transactions)
    {
        var customerBillingAccounts = new List<CustomerAccount>();
        foreach (var transaction in transactions.Transactions)
        {
            var charge = transaction.Value.Amount;
            var existingCustomerAccount = await GetObjectFromS3By(CreateS3FileKey(transaction.CustomerId));
            if (existingCustomerAccount == null)
            {
                existingCustomerAccount = AddNewAccount(transaction);
            }
            else
            {
                UpdateBalance(existingCustomerAccount.CreditAccount.Balance, charge);

                existingCustomerAccount.CreditAccount.Statements.Add(AddStatement(transaction, existingCustomerAccount.CreditAccount.Balance.Amount));
            }
            customerBillingAccounts.Add(existingCustomerAccount);
        }
        return customerBillingAccounts;
    }

    public async Task<CustomerAccount> GetCustomerAccount(string customerId)
    {
            var customerAccount = await GetObjectFromS3By(CreateS3FileKey(customerId));
            return customerAccount ?? new CustomerAccount();
    }

    private void UpdateBalance(Price balance, decimal charge)
    {
        balance.Amount = balance.Amount - charge;
    }

    private async Task<CustomerAccount?> GetObjectFromS3By(string key)
    {
        try
        {
            using var response = await _s3Client.GetObjectAsync(_s3Settings.Value.BucketName, key);
            if (response.HttpStatusCode is HttpStatusCode.NotFound)
            {
                _logger.ObjectNotFoundFromS3(_s3Settings.Value.BucketName, key);
                return default;
            }

            _logger.ReadingTheFileContentsFromS3(_s3Settings.Value.BucketName, key);
            return JsonSerializer.Deserialize<CustomerAccount>(response.ResponseStream, SerializerOptions.Default);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.ObjectNotFoundFromS3(_s3Settings.Value.BucketName, key);
            return default;
        }
    }

    private CustomerAccount AddNewAccount(Transaction singleTransaction)
    {
        var charge = singleTransaction.Value.Amount;
        var balance = new Price { Amount = StartingBalance - charge, CurrencyCode = CurrencyCodes.Gbp };

        return new CustomerAccount
        {
            CustomerId = singleTransaction.CustomerId,
            CreditAccount = new CreditAccount
            {
                Balance = balance,
                Statements = new List<Statement> { AddStatement(singleTransaction, balance.Amount) }
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

    private string CreateS3FileKey(string customerId)
    {
        return $"{_s3Settings.Value.Folder}/{_s3Settings.Value.FileName}_{customerId}{_s3Settings.Value.FileExtension}";
    }
}
