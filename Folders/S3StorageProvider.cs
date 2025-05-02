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

    public S3StorageProvider(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        Prefix = bucketName;
        ProviderKey = "s3://"; // Set the prefix to "s3" for S3 storage
    }    

    private async Task UploadToS3Async(string key, Stream stream)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = Prefix,
            Key = key,
            InputStream = stream
        };
        await _s3Client.PutObjectAsync(putRequest);
    }

    public override async Task<StorageId> StoreAsync(byte[] data)
    {
        var key = GenerateStorageId();      // e.g., "s3://fileapi1/2025/05/02/29199779604a4c498cb153dee1f682cc"
        using var stream = new MemoryStream(data);
        await UploadToS3Async(key, stream);
        return $"s3://{_bucketName}/{key}"; // Like "s3://bucket/path/to/file"
    }

    public override async Task<StorageId> StoreStreamAsync(Stream dataStream)
    {
        var key = GenerateStorageId();
        await UploadToS3Async(key, dataStream);
        return $"s3://{_bucketName}/{key}"; // Like "s3://bucket/path/to/file"
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        using var response = await _s3Client.GetObjectAsync(bucketName, storageId);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        var response = await _s3Client.GetObjectAsync(bucketName, storageId);
        return response.ResponseStream; // Caller should dispose
    }

    public override async Task DeleteAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        await _s3Client.DeleteObjectAsync(bucketName, storageId);
    }
}
