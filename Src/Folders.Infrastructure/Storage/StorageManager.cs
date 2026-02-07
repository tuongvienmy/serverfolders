
using Folders.Application.Abstractions;
using Folders.Core.Values;

namespace Folders.Infrastructure.Storage;
public class StorageManager : IStorageManager
{
    private readonly IStorageProviderRegistry _providerRegistry;
    public StorageManager(IStorageProviderRegistry providerRegistry)
    {
        _providerRegistry = providerRegistry;
    }
    public Task<StorageInfo> StoreStreamAsync(Stream dataStream, StorageProviderKey providerKey)
    {
        var provider = _providerRegistry.Resolve(providerKey);
        return provider.StoreStreamAsync(dataStream);
    }
    public Task<StorageInfo> StoreAsync(byte[] data, StorageProviderKey providerKey)
    {
        var provider = _providerRegistry.Resolve(providerKey);
        return provider.StoreAsync(data);
    }
    public Task<byte[]> RetrieveAsync(StorageId id)
    {
        var provider = _providerRegistry.Resolve(id.Provider);
        return provider.RetrieveAsync(id);
    }
    public Task<Stream> RetrieveStreamAsync(StorageId id)
    {
        var provider = _providerRegistry.Resolve(id.Provider);
        return provider.RetrieveStreamAsync(id);
    }
    public Task DeleteAsync(StorageId id)
    {
        var provider = _providerRegistry.Resolve(id.Provider);
        return provider.DeleteAsync(id);
    }
}