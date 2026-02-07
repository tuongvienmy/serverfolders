using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Folders.Core.S3;
using Folders.Core.Values;

namespace Tests.Folders.Core.StorageProvider.S3;

[TestClass]
public class S3StorageProviderTests
{
    private static IAmazonS3 _s3Client;
    private S3StorageProvider _s3StorageProvider;
    static readonly string BucketName = Environment.GetEnvironmentVariable("BucketName") ?? @"fileapi1";

    [TestInitialize]
    public void Setup()
    {
        ShowEnvironmentVariables();
        _s3Client = new AmazonS3Client(); // Uses environment variables or other default sources
        _s3StorageProvider = new S3StorageProvider(_s3Client, BucketName);
    }

    [TestMethod]
    public async Task StoreAsync_ShouldUploadDataAndReturnStorageId()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test byte array");

        // Act
        var storageId = await _s3StorageProvider.StoreAsync(data);

        // Assert
        Assert.IsNotNull(storageId);
        await CleanupObject(storageId);
    }

    [TestMethod]
    public async Task StoreStreamAsync_ShouldUploadStreamAndReturnStorageId()
    {
        // Arrange
        var dataStream = new MemoryStream(Encoding.UTF8.GetBytes("test stream"));

        // Act
        var storageId = await _s3StorageProvider.StoreStreamAsync(dataStream);

        // Assert
        Assert.IsNotNull(storageId);
        await CleanupObject(storageId);
    }

    [TestMethod]
    public async Task RetrieveAsync_ShouldReturnByteArray()
    {
        // Arrange
        await using var stream = await GetEmbeddedResourceStream();
        var originalSize = stream.Length;
        var storageId = await _s3StorageProvider.StoreStreamAsync(stream);

        // Act
        var result = await _s3StorageProvider.RetrieveAsync(storageId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(originalSize, result.Length);
        await CleanupObject(storageId);
    }

    [TestMethod]
    public async Task RetrieveStreamAsync_ShouldReturnStream()
    {
        // Arrange
        await using var stream = await GetEmbeddedResourceStream();
        var originalSize = stream.Length;
        var storageId = await _s3StorageProvider.StoreStreamAsync(stream);

        // Act
        await using (var result = await _s3StorageProvider.RetrieveStreamAsync(storageId))
        {
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(originalSize, result.Length);
        }
        await CleanupObject(storageId);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteObject()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test data");
        var storageId = await _s3StorageProvider.StoreAsync(data);

        // Act
        await _s3StorageProvider.DeleteAsync(storageId);

        // Assert
        // Verify that the object was deleted
        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = storageId
        };
        try
        {
            await _s3Client.GetObjectAsync(request);
            Assert.Fail("Expected exception not thrown.");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Expected exception
        }
    }

    private async Task CleanupObject(StorageId storageId)
    {
        await _s3StorageProvider.DeleteAsync(storageId);
    }
    private static void ShowEnvironmentVariables()
    {
        Console.WriteLine("Environment Variables:");
        Console.WriteLine($"AWS_ACCESS_KEY_ID: {Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")}");
        Console.WriteLine($"AWS_SECRET_ACCESS_KEY: {Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")}");
        Console.WriteLine($"AWS_REGION: {Environment.GetEnvironmentVariable("AWS_REGION")}");
        Console.WriteLine(new string('-', 40));
    }

    private static async Task<Stream> GetEmbeddedResourceStream()
    {
        Stream? stream = null;
        try
        {
            var assembly = typeof(S3StorageProviderTests).Assembly;
            stream = assembly.GetManifestResourceStream("FoldersTests.Resources.Moon.jpg");
            if (stream == null)
            {
                Assert.Fail("Resource not found.");
            }

            return stream;
        }
        catch
        {
            if (stream != null) await stream.DisposeAsync();
            throw;
        }
    }
}

