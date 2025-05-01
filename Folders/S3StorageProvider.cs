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

    public string ProviderKey => "AWS_S3";

    public S3StorageProvider(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }    

    private async Task UploadToS3Async(string key, Stream stream)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream
        };
        await _s3Client.PutObjectAsync(request);
    }

    private static string GenerateStorageKey() => Guid.NewGuid().ToString("N");

    public override async Task<StorageId> StoreAsync(byte[] data)
    {
        var key = GenerateStorageKey();
        using var stream = new MemoryStream(data);
        await UploadToS3Async(key, stream);
        return key;
    }

    public override async Task<StorageId> StoreStreamAsync(Stream dataStream)
    {
        var key = GenerateStorageKey();
        await UploadToS3Async(key, dataStream);
        return key;
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {
        using var response = await _s3Client.GetObjectAsync(_bucketName, storageId);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, storageId);
        return response.ResponseStream; // Caller should dispose
    }

    public override async Task DeleteAsync(StorageId storageId)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, storageId);     
    }
}
