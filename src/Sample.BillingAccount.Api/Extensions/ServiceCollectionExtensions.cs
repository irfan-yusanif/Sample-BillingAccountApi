using Sample.BillingAccount.Api.Configuration;
using Sample.BillingAccount.Api.Providers;
using Sample.BillingAccount.Api.Repositories;

namespace Sample.BillingAccount.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddS3Settings(this IServiceCollection services, IConfiguration configuration)
    {
        var s3Settings = configuration.GetSection(nameof(S3Settings));
        services.Configure<S3Settings>(s3Settings);
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IRequestHeaderProvider, RequestHeaderProvider>();
        services.AddAutoMapper(typeof(Startup));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IBillingAccountRepository, BillingAccountRepository>();

        return services;
    }
}
