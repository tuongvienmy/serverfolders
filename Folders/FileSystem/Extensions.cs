namespace Folders.Core.FileSystem;

internal static class Extensions
{
    public static string CreateFilePath(this StorageId storageId)
    {
        var directory = Path.GetDirectoryName(storageId.Path);
        ArgumentNullException.ThrowIfNull(directory, nameof (storageId));
        Directory.CreateDirectory(directory);
        return storageId.Path;
    }
}
