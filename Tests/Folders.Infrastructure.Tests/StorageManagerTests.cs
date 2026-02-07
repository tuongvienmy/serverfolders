using Folders.Application.Abstractions;
using Folders.Core.Values;
using Folders.Infrastructure.Storage;
using NSubstitute;

namespace Folders.Infrastructure.Tests;

[TestClass]
public class StorageManagerTests
{
    private IStorageProviderRegistry _registry;
    private IStorageProvider _provider;
    private StorageManager _manager;

    [TestInitialize]
    public void Setup()
    {
        _registry = Substitute.For<IStorageProviderRegistry>();
        _provider = Substitute.For<IStorageProvider>();
        _manager = new StorageManager(_registry);
    }

    [TestMethod]
    public async Task StoreStreamAsync_ShouldResolveProvider_AndCallStoreStreamAsync()
    {
        // Arrange
        StorageProviderKey key = "testProvider";

        var stream = new MemoryStream([1, 2, 3]);
        var expectedInfo = new StorageInfo(new StorageId(key, "path", "/file.txt"), "text/plain", 3);

        _registry.Resolve(key).Returns(_provider);
        _provider.StoreStreamAsync(stream).Returns(expectedInfo);

        // Act
        var result = await _manager.StoreStreamAsync(stream, key);

        // Assert
        Assert.AreEqual(expectedInfo, result);
        await _provider.Received(1).StoreStreamAsync(stream);
    }

    [TestMethod]
    public async Task StoreAsync_ShouldResolveProvider_AndCallStoreAsync()
    {
        // Arrange
        StorageProviderKey key = "testProvider";

        var data = new byte[] { 1, 2, 3 };
        var expectedInfo = new StorageInfo(new StorageId(key, "path", "/file.txt"), "text/plain",3);

        _registry.Resolve(key).Returns(_provider);
        _provider.StoreAsync(data).Returns(expectedInfo);

        // Act
        var result = await _manager.StoreAsync(data, key);

        // Assert
        Assert.AreEqual(expectedInfo, result);
        await _provider.Received(1).StoreAsync(data);
    }

    [TestMethod]
    public async Task RetrieveAsync_ShouldResolveProvider_AndCallRetrieveAsync()
    {
        // Arrange
        StorageProviderKey key = "testProvider";
        var storageId = new StorageId(key, "path", "/file.txt");
        var expectedData = new byte[] { 9, 8, 7 };

        _registry.Resolve(key).Returns(_provider);
        _provider.RetrieveAsync(storageId).Returns(expectedData);

        // Act
        var result = await _manager.RetrieveAsync(storageId);

        // Assert
        Assert.AreSame(expectedData, result);
        await _provider.Received(1).RetrieveAsync(storageId);
    }

    [TestMethod]
    public async Task RetrieveStreamAsync_ShouldResolveProvider_AndCallRetrieveStreamAsync()
    {
        // Arrange
        StorageProviderKey key = "testProvider";

        var storageId = new StorageId(key, "path", "/file.txt");
        var expectedStream = new MemoryStream(new byte[] { 4, 5, 6 });

        _registry.Resolve(key).Returns(_provider);
        _provider.RetrieveStreamAsync(storageId).Returns(expectedStream);

        // Act
        var result = await _manager.RetrieveStreamAsync(storageId);

        // Assert
        Assert.AreSame(expectedStream, result);
        await _provider.Received(1).RetrieveStreamAsync(storageId);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldResolveProvider_AndCallDeleteAsync()
    {
        // Arrange
        StorageProviderKey key = "testProvider";

        var storageId = new StorageId(key, "path", "/file.txt");

        _registry.Resolve(key).Returns(_provider);

        // Act
        await _manager.DeleteAsync(storageId);

        // Assert
        await _provider.Received(1).DeleteAsync(storageId);
    }
}

