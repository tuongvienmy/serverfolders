namespace Folders.Core;
public abstract class BaseStorageProvider: IStorageProvider
{
    protected virtual string Prefix { get; set; } = string.Empty;
    protected string GenerateKey()
    {
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid().ToString("N");
        var path = string.IsNullOrEmpty(Prefix)
            ? $"{now:yyyy/MM/dd}/"
            : $"{Prefix}/{now:yyyy/MM/dd}/";        

        return $"{path}{guid}";
    }
    public string ProviderKey { get; protected set; } = string.Empty;
    public abstract Task<StorageId> StoreAsync(byte[] data);
    public abstract Task<StorageId> StoreStreamAsync(Stream dataStream);
    public abstract Task<byte[]> RetrieveAsync(StorageId storageId);
    public abstract Task<Stream> RetrieveStreamAsync(StorageId storageId);
    public abstract Task DeleteAsync(StorageId storageId);
}
