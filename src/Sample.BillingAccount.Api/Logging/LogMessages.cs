namespace Sample.BillingAccount.Api.Logging;

public class LogMessages
{
    public static readonly Func<string, string> RequiredHeadersMissing = headers => $"{headers} headers(s) required";

    // S3 - Add
    public const string UploadingEntity = "Uploading credit account into bucket {bucketName} with key {key}";
    public const string StoredEntity = "Status Code: {statusCode}. Succesfully stored credit account in \n Bucket: {bucketName} \n Key: {key}";
    public const string FailedToStoreEntity = "Failed to store credit account. Check inner exception for details.";

    // S3 - GET
    public const string ObjectNotFoundFromS3 = "Not Found response from S3. Bucket: {bucketName} \n Key: {key}";
    public const string ReadingTheFileContentsFromS3 = "Reading the journey fares determinations file from S3. Bucket: {bucketName} \n Key: {key}";
}
