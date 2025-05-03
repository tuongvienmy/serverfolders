namespace Folders.Core;

/// <summary>
///  Provides the basic structure for a storage provider. The class API includes a couple optional attributes, which is somewhat biased by the knowledge of some welknown storage provider, e.g file system, S3...
/// </summary>
/// <remarks>Each storage provider should extend from this class and provide extended behaviours on the StorageId that make sense in the context of the provider</remarks>
public abstract class BaseStorageProvider: IStorageProvider
{
    protected virtual string Prefix { get; set; } = string.Empty;
    protected string ProviderKey { get; set; } = string.Empty;

    public BaseStorageProvider(string providerKey, string prefix)
    {
        ProviderKey = providerKey;
        Prefix = prefix;
    }
    protected virtual StorageId GenerateStorageId()
    {
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid().ToString("N");
        var path = string.IsNullOrEmpty(Prefix)
            ? $"{now:yyyy/MM/dd}/"
            : $"{Prefix}/{now:yyyy/MM/dd}/";

        return new StorageId(ProviderKey, $"{path}{guid}");

    }    
    public abstract Task<StorageId> StoreAsync(byte[] data);
    public abstract Task<StorageId> StoreStreamAsync(Stream dataStream);
    public abstract Task<byte[]> RetrieveAsync(StorageId storageId);
    public abstract Task<Stream> RetrieveStreamAsync(StorageId storageId);
    public abstract Task DeleteAsync(StorageId storageId);
}
