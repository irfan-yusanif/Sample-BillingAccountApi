using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.S3;
using Microsoft.OpenApi.Models;
using Sample.BillingAccount.Api.Extensions;
using Sample.BillingAccount.Api.Filters;
using Sample.BillingAccount.Api.Middleware;
using Sample.BillingAccount.Api.Serialization;

namespace Sample.BillingAccount.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHealthChecks();
        services.AddSwaggerGen(c =>
        {
            c.OperationFilter<HeaderFilter>();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = ApplicationInfo.ApplicationName,
                Version = $"v{ApplicationInfo.MajorVersion}"
            });
        });
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    opts.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
                    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
        services.AddAutoMapper(typeof(Startup).Assembly);
        services.AddS3Settings(Configuration);
        services.AddAWSService<IAmazonS3>();
        services.AddServices();
        services.AddRepositories();
        services.AddDateOnlyTimeOnlyStringConverters();
        services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app
            .UseHttpsRedirection()
            .UseSwagger()
            .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", ApplicationInfo.ApplicationName + $" v{ApplicationInfo.MajorVersion}"))
            .UseRouting()
            .UseMiddleware<ExceptionMiddleware>()
            .UseMiddleware<LogExtraPropertiesMiddleware>()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            })
            .UseHealthChecks("/diagnostics/healthcheck");
    }
}
