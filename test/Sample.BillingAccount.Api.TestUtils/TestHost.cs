using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sample.BillingAccount.Api.Repositories;
using Sample.BillingAccount.Api.TestUtils.Repositories;

namespace Sample.BillingAccount.Api.TestUtils;

public class TestHost : WebApplicationFactory<Startup>
{
    public TestHost()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IBillingAccountRepository, BillingAccountRepositoryTest>();
        });
    }
}
