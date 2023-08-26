using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sample.BillingAccount.Api.Configuration;
using Sample.BillingAccount.Api.Extensions;
using Sample.BillingAccount.Api.TestUtils.AutoData;
using Xunit;

namespace Sample.BillingAccount.Api.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [AutoMoqData]
    public void AddUnifiedLocationDataApiClient_ShouldReturnServicesWithResolvedDependencies(
        string bucketName,
        string folder,
        string fileName)
    {
        //Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "S3Settings:BucketName", bucketName },
                { "S3Settings:Folder", folder },
                { "S3Settings:FileName", fileName }
            }).Build();

        var services = new ServiceCollection();

        var serviceProvider = services.AddS3Settings(config).BuildServiceProvider();

        //Act
        var s3SettingOptions = serviceProvider.GetService<IOptions<S3Settings>>();

        //Assert
        s3SettingOptions!.Value.BucketName.Should().Be(bucketName);
        s3SettingOptions.Value.Folder.Should().Be(folder);
        s3SettingOptions.Value.FileName.Should().Be(fileName);
    }
}
