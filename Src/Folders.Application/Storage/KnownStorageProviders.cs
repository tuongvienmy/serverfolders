using Folders.Core.Values;

namespace Folders.Application.Storage;
public static class KnownStorageProviders
{
    public static readonly StorageProviderKey S3 = StorageProviderKey.S3;
    public static readonly StorageProviderKey Disk = StorageProviderKey.File;

    private static readonly Dictionary<StorageProviderKey, string> _friendlyNames = new()
    {
        [StorageProviderKey.S3] = "Amazon S3",
        [StorageProviderKey.File] = "Local Disk",
    };

    public static IReadOnlyCollection<StorageProviderKey> All => _friendlyNames.Keys.ToList();
    public static string? GetDisplayName(StorageProviderKey key) =>
        _friendlyNames.TryGetValue(key, out var name) ? name : key.ToString();
}

