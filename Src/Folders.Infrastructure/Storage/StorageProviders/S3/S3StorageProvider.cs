using Amazon.S3;
using Amazon.S3.Model;
using Folders.Core.Values;
using Folders.Application.Abstractions;

namespace Folders.Infrastructure.Storage.StorageProviders.S3;

public class S3StorageProvider : BaseStorageProvider
{
    private readonly IAmazonS3 _s3Client;

    public S3StorageProvider(IAmazonS3 client, string bucketName, IStoragePathStrategy pathStrategy)
        : base(StorageProviderKey.S3, bucketName, pathStrategy)
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

    public override async Task<StorageInfo> StoreAsync(byte[] data)
    {
        StorageId Id = GenerateStorageId();      // e.g., "s3://fileapi1/2025/05/02/29199779604a4c498cb153dee1f682cc"
        using var stream = new MemoryStream(data);
        var mimeType = MimeType.FromBuffer(data);
        await UploadToS3Async(Id.RelativePath, stream);
        return new StorageInfo(Id, mimeType, data.LongLength);
    }

    public override async Task<StorageInfo> StoreStreamAsync(Stream dataStream)
    {
        StorageId Id = GenerateStorageId();
        await UploadToS3Async(Id.RelativePath, dataStream);
        var mimeType = MimeType.FromStream(dataStream);
        return new StorageInfo(Id, mimeType, dataStream.Length);
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {       
        var request = new GetObjectRequest
        {
            BucketName = Prefix,
            Key = storageId.RelativePath
        };
        using var response = await _s3Client.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {        
        var request = new GetObjectRequest
        {
            BucketName = Prefix,
            Key = storageId.RelativePath
        };
        using var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream; // Caller should dispose
    }

    public override async Task DeleteAsync(StorageId storageId)
    {        
        var request = new DeleteObjectRequest()
        {
            BucketName = Prefix,
            Key = storageId.RelativePath
        };
        await _s3Client.DeleteObjectAsync(request);
    }
}
