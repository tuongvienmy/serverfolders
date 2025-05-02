using Amazon.S3;
using Amazon.S3.Model;
using Folders.Core;

namespace CloudFolders.Core;

//services.AddAWSService<IAmazonS3>();
//services.AddSingleton<IStorageProvider>(sp =>
//    new S3StorageProvider(
//        sp.GetRequiredService<IAmazonS3>(),
//        "your-s3-bucket-name"
//    ));

public class S3StorageProvider : BaseStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    //public string ProviderKey => "AWS_S3";

    public S3StorageProvider(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }    

    private async Task UploadToS3Async(string key, Stream stream)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream
        };
        await _s3Client.PutObjectAsync(putRequest);
    }

    private static string GenerateStorageKey() => Guid.NewGuid().ToString("N");

    public override async Task<StorageId> StoreAsync(byte[] data)
    {
        var key = GenerateStorageKey();
        using var stream = new MemoryStream(data);
        await UploadToS3Async(key, stream);
        return $"s3://{_bucketName}/{key}"; // Like "s3://bucket/path/to/file"
    }

    public override async Task<StorageId> StoreStreamAsync(Stream dataStream)
    {
        var key = GenerateStorageKey();
        await UploadToS3Async(key, dataStream);
        return $"s3://{_bucketName}/{key}"; // Like "s3://bucket/path/to/file"
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = storageId.Path
        };
        using var response = await _s3Client.GetObjectAsync(getRequest);

        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = storageId.Path
        };
        using var response = await _s3Client.GetObjectAsync(getRequest);
        return response.ResponseStream; // Caller should dispose
    }

    public override async Task DeleteAsync(StorageId storageId)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storageId.Path
        };
        await _s3Client.DeleteObjectAsync(deleteRequest);     
    }
}
