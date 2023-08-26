using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;

namespace Sample.BillingAccount.Api.TestUtils;

[ExcludeFromCodeCoverage]
public static class LoggerVerifier
{
    public static void VerifyMessageLogged(
        this ITestLoggerFactory logger,
        LogLevel? expectedLogLevel = null,
        string? expectedMessage = null,
        int? expectedEventId = null,
        string? expectedEventName = null,
        Type? expectedExceptionType = null,
        Times? times = null)
    {
        var matchingLogMessages = logger.Sink.LogEntries;
        if (expectedLogLevel != null)
            matchingLogMessages = matchingLogMessages.Where(x => x.LogLevel == expectedLogLevel);

        if (expectedMessage != null)
            matchingLogMessages = matchingLogMessages.Where(x => x.Message!.Contains(expectedMessage));

        if (expectedEventId != null)
            matchingLogMessages = matchingLogMessages.Where(x => x.EventId.Id == expectedEventId);

        if (expectedEventName != null)
            matchingLogMessages = matchingLogMessages.Where(x => x.EventId.Name == expectedEventName);

        if (expectedExceptionType != null)
            matchingLogMessages = matchingLogMessages.Where(x => x.Exception?.GetType() == expectedExceptionType);

        var count = matchingLogMessages.Count();
        times ??= Times.Once();
        times.Value.Validate(count).Should().BeTrue($"{expectedEventName} should match {times.Value} but actually matched {count} times");
    }
}
