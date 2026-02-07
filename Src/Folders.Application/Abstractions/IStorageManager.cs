using Folders.Core.Values;

namespace Folders.Application.Abstractions;
public interface IStorageManager
{
    Task<StorageInfo> StoreStreamAsync(Stream dataStream, StorageProviderKey providerKey);
    Task<StorageInfo> StoreAsync(byte[] data, StorageProviderKey providerKey);
    Task<byte[]> RetrieveAsync(StorageId id);
    Task<Stream> RetrieveStreamAsync(StorageId id);
    Task DeleteAsync(StorageId id);
}

