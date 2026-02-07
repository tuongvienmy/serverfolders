using NSubstitute;
using Folders.Application.Abstractions;
using Folders.Core.Values;
using Folders.Infrastructure.Storage.StorageProviders;

namespace Folders.Infrastructure.Tests;

[TestClass]
public class BaseStorageProviderTests
{
    private class FakeStorageProvider : BaseStorageProvider
    {
        public FakeStorageProvider(StorageProviderKey key, string prefix, IStoragePathStrategy strategy)
            : base(key, prefix, strategy) { }

        public override Task<StorageInfo> StoreAsync(byte[] data) => Task.FromResult<StorageInfo>(null);
        public override Task<StorageInfo> StoreStreamAsync(Stream dataStream) => Task.FromResult<StorageInfo>(null);
        public override Task<byte[]> RetrieveAsync(StorageId storageId) => Task.FromResult<byte[]>(null);
        public override Task<Stream> RetrieveStreamAsync(StorageId storageId) => Task.FromResult<Stream>(null);
        public override Task DeleteAsync(StorageId storageId) => Task.CompletedTask;

        public StorageId CallGenerateStorageId() => base.GenerateStorageId();
    }

    [TestMethod]
    public void GenerateStorageId_ShouldUsePathStrategyWithPrefix()
    {
        // Arrange
        StorageProviderKey key = "test";
        var prefix = "base";
        
        var mockStrategy = Substitute.For<IStoragePathStrategy>();
        mockStrategy.GenerateRelativePath().Returns("path/generated.txt");

        var provider = new FakeStorageProvider(key, prefix, mockStrategy);

        // Act
        var storageId = provider.CallGenerateStorageId();

        // Assert
        Assert.AreEqual(key, storageId.Provider);
        Assert.AreEqual("path/generated.txt", storageId.RelativePath);
        mockStrategy.Received(1).GenerateRelativePath();
    }

    [TestMethod]
    public void Constructor_ShouldThrow_WhenPathStrategyIsNull()
    {
        // Arrange
        StorageProviderKey key = "test";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FakeStorageProvider(key, "prefix", null));
    }
}
