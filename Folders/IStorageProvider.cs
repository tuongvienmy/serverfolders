
namespace Folders.Core;
public interface IStorageProvider
{
    string ProviderKey { get; }
    Task<StorageId> StoreAsync(byte[] data);
    Task<StorageId> StoreStreamAsync(Stream dataStream);
    Task<byte[]> RetrieveAsync(StorageId storageId);    
    Task<Stream> RetrieveStreamAsync(StorageId storageId);
    Task DeleteAsync(StorageId storageId);        
}
public interface IStorageProviderFactory
{
    IStorageProvider GetProvider(StorageId storageId);
}
