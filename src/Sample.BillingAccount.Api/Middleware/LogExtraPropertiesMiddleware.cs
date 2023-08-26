using Sample.BillingAccount.Api.Extensions;
using Sample.BillingAccount.Api.Logging;
using Sample.BillingAccount.Api.Providers;

namespace Sample.BillingAccount.Api.Middleware;

public class LogExtraPropertiesMiddleware
{
    private readonly RequestDelegate _next;

    public LogExtraPropertiesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRequestHeaderProvider requestHeaderProvider, ILogger<LogExtraPropertiesMiddleware> logger)
    {
        using var loggerContext = logger.BeginScope(new Dictionary<string, object?>
                                                {
                                                     {ExtraPropertiesLogKeys.ConversationId, requestHeaderProvider.ConversationId()},
                                                });
        {
            await _next.Invoke(context);
        }
    }
}
