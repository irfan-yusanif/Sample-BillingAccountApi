namespace Sample.BillingAccount.Api.Providers;

public interface IRequestHeaderProvider
{
    string? GetHeaderValue(string key);
}

public class RequestHeaderProvider : IRequestHeaderProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestHeaderProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetHeaderValue(string key) =>
        _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(key, out var stringValues)
            ? stringValues.First()
            : null;
}
