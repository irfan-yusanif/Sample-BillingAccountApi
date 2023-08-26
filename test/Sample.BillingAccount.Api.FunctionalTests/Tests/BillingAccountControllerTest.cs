using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NScenario;
using Sample.BillingAccount.Api.Constants;
using Sample.BillingAccount.Api.Model.Internal;
using Sample.BillingAccount.Api.Model.Request;
using Sample.BillingAccount.Api.Repositories;
using Sample.BillingAccount.Api.Serialization;
using Sample.BillingAccount.Api.TestUtils;
using Sample.BillingAccount.Api.TestUtils.AutoData;
using Xunit;

namespace Sample.BillingAccount.Api.FunctionalTests.Tests;

public class BillingAccountControllerTest : IClassFixture<TestHost>
{
    private readonly HttpClient _client;
    private const string _transactionsUrl = "/BillingAccount/transactions";
    private string _getCustomerAccountUrl(string customerId) => $"/BillingAccount/{customerId}";

    public BillingAccountControllerTest(TestHost testHost)
    {
        _client = testHost.CreateClient();
    }

    [Theory, AutoMoqData]
    public async Task Save_OnSuccess_ReturnsNoContent(
        TransactionsRequest request,
        string conversationId)
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        var json = JsonSerializer.Serialize(request, SerializerOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, _transactionsUrl)
        {
            Content = content
        };
        requestMessage.Headers.Add(Headers.ConversationId, conversationId);

        // Act
        await scenario.Step("Given a valid list of TransactionsRequest when Save endpoint is called",
            async () =>
            {
                response = await _client.SendAsync(requestMessage);
            });

        await scenario.Step("Then response status code should be NoContent",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
            });
    }

    [Theory, AutoMoqData]
    public async Task Save_MissingConversationId_ReturnsBadRequest(
        TransactionsRequest request
    )
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        var json = JsonSerializer.Serialize(request, SerializerOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, _transactionsUrl)
        {
            Content = content
        };

        // Act
        await scenario.Step("Given a valid list of TransactionsRequest But missing ConversationId when Save endpoint is called",
            async () =>
            {
                response = await _client.SendAsync(requestMessage);
            });

        await scenario.Step("Then response status code should be BadRequest",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            });
    }

    [Theory, AutoMoqData]
    public async Task Save_ThrowException_Returns500(
        TransactionsRequest request,
        string conversationId,
        Exception ex,
        [Frozen] TestHost testHost,
        [Frozen] Mock<IBillingAccountRepository> repos)
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        repos.Setup(repo => repo.SaveBillingAccounts(It.IsAny<List<CustomerAccount>>())).Throws(ex);

        var client = testHost.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IBillingAccountRepository>(repos.Object);
            });
        }).CreateClient();

        var json = JsonSerializer.Serialize(request, SerializerOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, _transactionsUrl)
        {
            Content = content
        };
        requestMessage.Headers.Add(Headers.ConversationId, conversationId);

        // Act
        await scenario.Step("Given a valid list of TransactionsRequest when Save endpoint is called",
            async () =>
            {
                response = await client.SendAsync(requestMessage);
            });

        await scenario.Step("Returns 500 when exception happens to repository side",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            });
    }

    [Theory, AutoMoqData]
    public async Task GetCustomerAccount_GivenAValidCustomerId_WhenAccountExistsForThatCustomer_ThenReturnsCustomerAccount(
        string customerId,
        string conversationId)
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, _getCustomerAccountUrl(customerId));
        requestMessage.Headers.Add(Headers.ConversationId, conversationId);

        // Act
        await scenario.Step("Given a valid request with customerId when GetCustomerAccount endpoint is called",
            async () =>
            {
                response = await _client.SendAsync(requestMessage);
            });

        await scenario.Step("Then response status code should be OK",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.OK);
            });
    }

    [Theory, AutoMoqData]
    public async Task GetCustomerAccount_WhenConversationIdIsMissing_ThenReturnsBadRequest(
        string customerId
    )
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, _getCustomerAccountUrl(customerId));

        // Act
        await scenario.Step("Given a valid request with customerId But missing ConversationId when GetCustomerAccount endpoint is called",
            async () =>
            {
                response = await _client.SendAsync(requestMessage);
            });

        await scenario.Step("Then response status code should be BadRequest",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            });
    }

    [Theory, AutoMoqData]
    public async Task GetCustomerAccount_WhenExceptionOccurs_ThenReturns500(
        string customerId,
    string conversationId,
        Exception ex,
        [Frozen] TestHost testHost,
        [Frozen] Mock<IBillingAccountRepository> repos)
    {
        // Arrange
        var scenario = TestScenarioFactory.Default();
        HttpResponseMessage? response = null;

        repos.Setup(repo => repo.GetCustomerAccount(It.IsAny<string>())).Throws(ex);

        var client = testHost.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IBillingAccountRepository>(repos.Object);
            });
        }).CreateClient();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, _getCustomerAccountUrl(customerId));
        requestMessage.Headers.Add(Headers.ConversationId, conversationId);

        // Act
        await scenario.Step("Given a valid request with customerId when GetCustomerAccount endpoint is called",
            async () =>
            {
                response = await client.SendAsync(requestMessage);
            });

        await scenario.Step("Then returns 500 when exception occurs in repository",
            () =>
            {
                response!.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            });
    }
}
