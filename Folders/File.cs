namespace Folders.Core;

public class File : FolderItem
{
    public StorageId StorageId { get; protected set; } = string.Empty;
    public long Size { get; protected set; } = 0;
    public string MimeType { get; protected set; } = string.Empty;    
    private File(string name, string mimeType, StorageId storageId) : base(name)
    {
        MimeType = mimeType;
        StorageId = storageId;
    }
    
    public static File Add(string name, string mimeType, byte[] data, IStorageProvider storageProvider)
    {        
        string storageId = storageProvider.StoreAsync(data).GetAwaiter().GetResult();

        return new File(name, mimeType, storageId);
    }
    public static File Add(string name, string mimeType, Stream data, IStorageProvider storageProvider)
    {
        string storageId = storageProvider.StoreStreamAsync(data).GetAwaiter().GetResult();

        return new File(name, mimeType, storageId);
    }
    public static File Add(FileInfo fileInfo, IStorageProvider storageProvider)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        if (!fileInfo.Exists) { throw new ArgumentException("File not exist."); }

        FileStream stream = fileInfo.OpenRead();

        return Add(fileInfo.Name, Core.MimeType.FromFileName(fileInfo.Extension), stream, storageProvider);
    }
    
}

