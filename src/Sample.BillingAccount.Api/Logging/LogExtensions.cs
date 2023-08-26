namespace Sample.BillingAccount.Api.Logging;

public static partial class LogExtensions
{
    // S3 - Add
    [LoggerMessage(LogEvents.UploadingEntity, LogLevel.Information, LogMessages.UploadingEntity)]
    public static partial void UploadingEntity(this ILogger logger, string bucketName, string key);

    [LoggerMessage(LogEvents.StoredEntity, LogLevel.Information, LogMessages.StoredEntity)]
    public static partial void StoredEntity(this ILogger logger, string statusCode, string bucketName, string key);

    [LoggerMessage(LogEvents.FailedToStoreEntity, LogLevel.Error, LogMessages.FailedToStoreEntity)]
    public static partial void FailedToStoreEntity(this ILogger logger, Exception e);

    // S3 - GET
    [LoggerMessage(LogEvents.ObjectNotFoundFromS3, LogLevel.Warning, LogMessages.ObjectNotFoundFromS3)]
    public static partial void ObjectNotFoundFromS3(this ILogger logger, string bucketName, string key);

    [LoggerMessage(LogEvents.ReadingTheFileContentsFromS3, LogLevel.Information, LogMessages.ReadingTheFileContentsFromS3)]
    public static partial void ReadingTheFileContentsFromS3(this ILogger logger, string bucketName, string key);
}
