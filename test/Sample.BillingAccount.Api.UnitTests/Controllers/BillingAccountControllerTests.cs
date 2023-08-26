using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sample.BillingAccount.Api.Controllers;
using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Internal;
using Sample.BillingAccount.Api.Model.Request;
using Sample.BillingAccount.Api.Repositories;
using Sample.BillingAccount.Api.TestUtils.AutoData;
using Xunit;

namespace Sample.BillingAccount.Api.UnitTests.Controllers;

public class BillingAccountControllerTests
{
    [Theory, AutoMoqData]
    public async Task Save_OnSuccess_ReturnsNoContent(
        [Frozen] Mock<IBillingAccountRepository> repositoryMock,
        TransactionsRequest request,
        TransactionsDto transactionsDto,
        List<CustomerAccount> customerAccounts,
        [Frozen] Mock<IMapper> mapperMock,
        BillingAccountController sut)
    {
        // Arrange
        mapperMock.Setup(m => m.Map<TransactionsDto>(request)).Returns(transactionsDto);
        repositoryMock.Setup(x => x.GetAccountAndAddTransaction(transactionsDto)).ReturnsAsync(customerAccounts);
        repositoryMock.Setup(x => x.SaveBillingAccounts(customerAccounts));

        // Act
        var result = await sut.Save(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        repositoryMock.Verify(x=>x.GetAccountAndAddTransaction(transactionsDto), Times.Once);
        repositoryMock.Verify(x => x.SaveBillingAccounts(customerAccounts), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task GetCustomerAccount_OnSuccess_ReturnsOk(
        [Frozen] Mock<IBillingAccountRepository> repositoryMock,
        string customerId,
        CustomerAccount customerAccount,
        BillingAccountController sut)
    {
        // Arrange
        repositoryMock.Setup(x => x.GetCustomerAccount(customerId)).ReturnsAsync(customerAccount);

        // Act
        var result = await sut.GetCustomerAccount(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        objectResult.Value.Should().BeOfType<CustomerAccount>();

        repositoryMock.Verify(x => x.GetCustomerAccount(customerId), Times.Once);
    }
}
