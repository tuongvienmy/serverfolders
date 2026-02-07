using Folders.Core.Values;

namespace Folders.Application.Abstractions;
public interface IStorageProviderRegistry
{
    IStorageProvider Resolve(StorageProviderKey key);
    bool IsRegistered(StorageProviderKey key);
    IReadOnlyCollection<StorageProviderKey> RegisteredKeys { get; }
}

