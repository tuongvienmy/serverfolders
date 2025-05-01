
namespace Folders.Core;

internal class StorageProviderFactory : IStorageProviderFactory
{
    private readonly Dictionary<string, IStorageProvider> _providers = [];
    public StorageProviderFactory()
    {
        // Register storage providers here
        // Example: _providers.Add("local", new LocalStorageProvider());
        // Example: _providers.Add("azure", new AzureStorageProvider());
    }
    public IStorageProvider GetProvider(string storageId)
    {
        var scheme = new Uri(storageId).Scheme;
        if (_providers.TryGetValue(scheme, out var provider))
        {
            return provider;
        }
        throw new NotSupportedException($"Storage provider for {storageId} is not supported.");
    }

    public IStorageProvider GetProvider(StorageId storageId)
    {
        throw new NotImplementedException();
    }
}