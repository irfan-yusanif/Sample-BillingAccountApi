namespace Sample.BillingAccount.Api.Logging;

public class LogEvents
{
    // S3 - Add
    public const int UploadingEntity = 1001;
    public const int StoredEntity = 1002;
    public const int FailedToStoreEntity = 1003;

    // S3 - GET
    public const int ObjectNotFoundFromS3 = 1011;
    public const int ReadingTheFileContentsFromS3 = 1012;
}

