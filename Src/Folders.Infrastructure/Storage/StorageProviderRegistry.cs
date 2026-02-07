using Folders.Core.Values;
using Folders.Application.Abstractions;
using System.Collections.Concurrent;


namespace Folders.Infrastructure.Storage;
public class StorageProviderRegistry : IStorageProviderRegistry
{
    private readonly ConcurrentDictionary<StorageProviderKey, IStorageProvider> _providers;

    public StorageProviderRegistry(IEnumerable<IStorageProvider> providers)
    {
        _providers = new ConcurrentDictionary<StorageProviderKey, IStorageProvider>(
            providers.Select(p => new KeyValuePair<StorageProviderKey, IStorageProvider>(p.StorageProviderKey, p))
        );
    }

    public IStorageProvider Resolve(StorageProviderKey key)
    {
        if (_providers.TryGetValue(key, out var provider))
            return provider;

        throw new KeyNotFoundException($"No storage provider registered with key '{key.Value}'.");
    }

    public bool IsRegistered(StorageProviderKey key) => _providers.ContainsKey(key);

    public IReadOnlyCollection<StorageProviderKey> RegisteredKeys => _providers.Keys.ToList();
}
