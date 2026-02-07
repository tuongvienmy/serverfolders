using Folders.Core.Values;

namespace Folders.Infrastructure.Storage.StorageProviders.FileSystem;

internal static class Extensions
{
    public static string CreateFilePath(this StorageId storageId)
    {
        var directory = Path.GetDirectoryName(storageId.RelativePath);
        ArgumentNullException.ThrowIfNull(directory, nameof (storageId));
        Directory.CreateDirectory(directory);
        return storageId.RelativePath;
    }
}
