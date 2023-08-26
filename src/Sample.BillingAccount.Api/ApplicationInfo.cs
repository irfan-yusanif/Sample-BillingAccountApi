using System.Net.Http.Headers;
using System.Reflection;

namespace Sample.BillingAccount.Api;

public static class ApplicationInfo
{
    public static readonly string ApplicationName;
    public static readonly int? MajorVersion;
    public static readonly ProductInfoHeaderValue UserAgent;
    public static readonly string SemanticVersion;

    static ApplicationInfo()
    {
        var assembly = typeof(Startup).Assembly;

        ApplicationName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Sample.BillingAccount.Api";
        MajorVersion = assembly.GetName().Version?.Major;
        SemanticVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
        UserAgent = new ProductInfoHeaderValue(ApplicationName, SemanticVersion);
    }
}
