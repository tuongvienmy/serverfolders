using Folders.Application.Abstractions;
using Folders.Core.Values;

namespace Folders.Infrastructure.Storage.StorageProviders.InMemory;
public class InMemoryStorageProvider : BaseStorageProvider
{
    private static readonly Dictionary<string, byte[]> _storage = new();    
    public InMemoryStorageProvider(string scope, IStoragePathStrategy pathStrategy)
        : base(StorageProviderKey.Memory, scope, pathStrategy)
    {
    }
    public override Task<StorageInfo> StoreAsync(byte[] data)
    {
        StorageId id = GenerateStorageId();
        _storage[id.RelativePath] = data;
        var mimeType = MimeType.FromBuffer(data);
        return Task.FromResult(new StorageInfo(id, mimeType, data.LongLength));
    }
    public override Task<StorageInfo> StoreStreamAsync(Stream dataStream)
    {
        StorageId id = GenerateStorageId();
        using var memoryStream = new MemoryStream();
        dataStream.CopyTo(memoryStream);
        _storage[id.RelativePath] = memoryStream.ToArray();
        var mimeType = MimeType.FromStream(memoryStream);
        return Task.FromResult(new StorageInfo(id, mimeType, memoryStream.Length));
    }
    public override Task<byte[]> RetrieveAsync(StorageId storageId)
    {
        if (_storage.TryGetValue(storageId.RelativePath, out var data))
        {
            return Task.FromResult(data);
        }
        throw new KeyNotFoundException($"Storage ID {storageId} not found.");
    }
    public override Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {
        if (_storage.TryGetValue(storageId.Value, out var data))
        {
            return Task.FromResult<Stream>(new MemoryStream(data));
        }
        throw new KeyNotFoundException($"Storage ID {storageId} not found.");
    }
    public override Task DeleteAsync(StorageId storageId)
    {
        if (_storage.ContainsKey(storageId.Value))
        {
            _storage.Remove(storageId.Value);
        }
        return Task.CompletedTask;
    }
}
