using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Folders.Core.Values;
using Folders.Infrastructure.Storage.StorageProviders.S3;
using System.Text;

namespace Folders.Infrastructure.Tests.Integration;
[TestClass]
public class S3StorageProviderTests
{
    private static IAmazonS3 _s3Client;
    private readonly string BucketName = Environment.GetEnvironmentVariable("BucketName") ?? @"fileapi1";
    private S3StorageProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        //ShowEnvironmentVariables();
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.APSoutheast2);
        _provider = new S3StorageProvider(_s3Client, BucketName, new DateBasedPathStrategy());
    }

    [TestMethod]
    public async Task StoreAsync_ShouldUploadDataAndReturnStorageId()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test byte array");

        // Act
        var storageInfo = await _provider.StoreAsync(data);

        Console.WriteLine($"Stored object with StorageId: {storageInfo.StorageId}");
        Console.WriteLine($"MimeType: {storageInfo.MimeType}");
        Console.WriteLine($"Size: {storageInfo.Size} bytes");

        // Assert
        Assert.IsNotNull(storageInfo);
        await CleanupObject(storageInfo.StorageId);
    }
    private async Task CleanupObject(StorageId storageId)
    {
        await _provider.DeleteAsync(storageId);
    }
    private static void ShowEnvironmentVariables()
    {
        Console.WriteLine("Environment Variables:");
        Console.WriteLine($"AWS_ACCESS_KEY_ID: {Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")}");
        Console.WriteLine($"AWS_SECRET_ACCESS_KEY: {Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")}");
        Console.WriteLine($"AWS_REGION: {Environment.GetEnvironmentVariable("AWS_REGION")}");
        Console.WriteLine(new string('-', 40));
    }
}
