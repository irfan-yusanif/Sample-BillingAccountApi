using System.Diagnostics.CodeAnalysis;

namespace Sample.BillingAccount.Api.Configuration;

[ExcludeFromCodeCoverage(Justification = "Don't test POCO's")]
public class S3Settings
{
    public string BucketName { get; init; } = string.Empty;
    public string Folder { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
}