using Folders.Core.Values;

namespace Folders.Application.Abstractions;
public interface IStorageProvider
{
    StorageProviderKey StorageProviderKey { get; set; }
    Task<StorageInfo> StoreAsync(byte[] data);
    Task<StorageInfo> StoreStreamAsync(Stream dataStream);
    Task<byte[]> RetrieveAsync(StorageId storageId);    
    Task<Stream> RetrieveStreamAsync(StorageId storageId);
    Task DeleteAsync(StorageId storageId);        
}
