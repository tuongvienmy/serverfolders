using Folders.Application.Abstractions;
using Folders.Infrastructure.Storage.StorageProviders.FileSystem;
using NSubstitute;
using System.Text;

namespace Folders.Infrastructure.Tests.Integration;

[TestClass]
public class FileSystemStorageProviderTests
{
    private string _tempDir;
    private IStoragePathStrategy _pathStrategy;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _pathStrategy = Substitute.For<IStoragePathStrategy>();
        _pathStrategy.GenerateRelativePath()
            .Returns(callInfo =>
            {
                return "tests\\TestFile.txt";
                
            });
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [TestMethod]
    public async Task StoreAsync_ShouldWriteFile_AndReturnStorageInfo()
    {
        // Arrange
        var provider = new FileSystemStorageProvider(_tempDir, _pathStrategy);
        var data = Encoding.UTF8.GetBytes("Hello World");

        // Act
        var info = await provider.StoreAsync(data);

        // Assert
        Assert.IsTrue(File.Exists(info.StorageId.RelativePath));
        var fileData = await File.ReadAllBytesAsync(info.StorageId.RelativePath);
        CollectionAssert.AreEqual(data, fileData);
        Assert.AreEqual(data.LongLength, info.Size);
    }

    [TestMethod]
    public async Task RetrieveAsync_ShouldReturnStoredData()
    {
        // Arrange
        var provider = new FileSystemStorageProvider(_tempDir, _pathStrategy);
        var data = Encoding.UTF8.GetBytes("Hello Retrieve");
        var info = await provider.StoreAsync(data);

        // Act
        var retrieved = await provider.RetrieveAsync(info.StorageId);

        // Assert
        CollectionAssert.AreEqual(data, retrieved);
    }

    [TestMethod]
    public async Task StoreStreamAsync_ShouldWriteFile_AndReturnStorageInfo()
    {
        // Arrange
        var provider = new FileSystemStorageProvider(_tempDir, _pathStrategy);
        var streamData = Encoding.UTF8.GetBytes("Stream Data");
        using var stream = new MemoryStream(streamData);

        // Act
        var info = await provider.StoreStreamAsync(stream);

        // Assert
        Assert.IsTrue(File.Exists(info.StorageId.RelativePath));
        var fileData = await File.ReadAllBytesAsync(info.StorageId.RelativePath);
        CollectionAssert.AreEqual(streamData, fileData);
    }

    [TestMethod]
    public async Task RetrieveStreamAsync_ShouldReturnStreamWithData()
    {
        // Arrange
        var provider = new FileSystemStorageProvider(_tempDir, _pathStrategy);
        var data = Encoding.UTF8.GetBytes("Stream Retrieval");
        var info = await provider.StoreAsync(data);

        // Act
        using var resultStream = await provider.RetrieveStreamAsync(info.StorageId);
        var buffer = new byte[data.Length];
        await resultStream.ReadAsync(buffer, 0, buffer.Length);

        // Assert
        CollectionAssert.AreEqual(data, buffer);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        // Arrange
        var provider = new FileSystemStorageProvider(_tempDir, _pathStrategy);
        var data = Encoding.UTF8.GetBytes("Delete Me");
        var info = await provider.StoreAsync(data);

        // Act
        await provider.DeleteAsync(info.StorageId);

        // Assert
        Assert.IsFalse(File.Exists(info.StorageId.RelativePath));
    }
}




