using Folders.Application.Abstractions;
using Folders.Core.Values;

namespace Folders.Infrastructure.Storage.StorageProviders;

public abstract class BaseStorageProvider: IStorageProvider
{
    protected virtual string Prefix { get; set; } = string.Empty;    
    protected IStoragePathStrategy PathStrategy { get; }
    public StorageProviderKey StorageProviderKey { get; set; }
    public BaseStorageProvider(StorageProviderKey storageProviderKey, string prefix, IStoragePathStrategy pathStrategy)
    {
        StorageProviderKey = storageProviderKey;
        Prefix = prefix ?? string.Empty;
        PathStrategy = pathStrategy ?? throw new ArgumentNullException(nameof(pathStrategy));
    }
    protected virtual StorageId GenerateStorageId()
    {        
        var path = PathStrategy.GenerateRelativePath();
        return new StorageId(StorageProviderKey, Prefix, path);

    }    
    public abstract Task<StorageInfo> StoreAsync(byte[] data);
    public abstract Task<StorageInfo> StoreStreamAsync(Stream dataStream);
    public abstract Task<byte[]> RetrieveAsync(StorageId storageId);
    public abstract Task<Stream> RetrieveStreamAsync(StorageId storageId);
    public abstract Task DeleteAsync(StorageId storageId);
}
