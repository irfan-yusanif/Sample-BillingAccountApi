using Sample.BillingAccount.Api.Constants;
using Sample.BillingAccount.Api.Providers;

namespace Sample.BillingAccount.Api.Extensions;

public static class RequestHeaderProviderExtensions
{
    public static string ConversationId(this IRequestHeaderProvider requestHeaderProvider)
    {
        return requestHeaderProvider.GetHeaderValue(Headers.ConversationId) ?? string.Empty;
    }
}
