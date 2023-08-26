using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Sample.BillingAccount.Api.Constants;
using Sample.BillingAccount.Api.Extensions;
using Sample.BillingAccount.Api.Providers;
using Sample.BillingAccount.Api.TestUtils.AutoData;
using Xunit;

namespace Sample.BillingAccount.Api.UnitTests.Extensions;

public class RequestHeaderProviderExtensionsTests
{
    private readonly IFixture _fixture;

    public RequestHeaderProviderExtensionsTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Theory, AutoMoqData]
    public void ConversationId_WhenHeaderExists_ShouldReturnHeaderValue(string conversationId)
    {
        // Arrange
        var requestHeaderProvider = _fixture.Create<IRequestHeaderProvider>();
        Mock.Get(requestHeaderProvider)
            .Setup(x => x.GetHeaderValue(Headers.ConversationId))
            .Returns(conversationId);

        // Act
        var result = requestHeaderProvider.ConversationId();

        // Assert
        result.Should().Be(conversationId);
    }

    [Fact]
    public void ConversationId_WhenHeaderDoesNotExist_ShouldReturnEmptyString()
    {
        // Arrange
        var requestHeaderProvider = _fixture.Create<IRequestHeaderProvider>();
        Mock.Get(requestHeaderProvider)
            .Setup(x => x.GetHeaderValue(Headers.ConversationId))
            .Returns(value: null);

        // Act
        var result = requestHeaderProvider.ConversationId();

        // Assert
        result.Should().BeEmpty();
    }
}
