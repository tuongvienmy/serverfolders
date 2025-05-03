using Amazon.S3;
using Amazon.S3.Model;
using Folders.Core;

namespace Folders.Core.S3;

//services.AddAWSService<IAmazonS3>();
//services.AddSingleton<IStorageProvider>(sp =>
//    new S3StorageProvider(
//        sp.GetRequiredService<IAmazonS3>(),
//        "your-s3-bucket-name"
//    ));

public class S3StorageProvider : BaseStorageProvider
{
    private readonly IAmazonS3 _s3Client;

    public S3StorageProvider(IAmazonS3 client, string bucketName): base("s3", bucketName)
    {
        _s3Client = client ?? throw new ArgumentNullException(nameof(client));        
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
        StorageId Id = GenerateStorageId();      // e.g., "s3://fileapi1/2025/05/02/29199779604a4c498cb153dee1f682cc"
        using var stream = new MemoryStream(data);
        await UploadToS3Async(Id.Path, stream);
        return Id;
    }

    public override async Task<StorageId> StoreStreamAsync(Stream dataStream)
    {
        StorageId Id = GenerateStorageId();
        await UploadToS3Async(Id.Path, dataStream);
        return Id;
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        //using var response = await _s3Client.GetObjectAsync(bucketName, storageId.Path);

        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = storageId.Path
        };
        using var response = await _s3Client.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        //var response = await _s3Client.GetObjectAsync(bucketName, storageId.Path);
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = storageId.Path
        };
        using var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream; // Caller should dispose
    }

    public override async Task DeleteAsync(StorageId storageId)
    {
        var bucketName = storageId.GetBucketNameFrom();
        //await _s3Client.DeleteObjectAsync(bucketName, storageId.Path);
        var request = new DeleteObjectRequest()
        {
            BucketName = bucketName,
            Key = storageId.Path
        };
        await _s3Client.DeleteObjectAsync(request);
    }
}
