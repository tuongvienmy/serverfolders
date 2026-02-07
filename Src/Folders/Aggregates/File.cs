using Folders.Core.Values;

namespace Folders.Core.Aggregates;

public class File : FolderItem    
{
    private File():base("New File") { } // required by EF
    public File(string name) : base(name) { }
    public StorageInfo StorageInfo { get; private set; } = StorageInfo.Empty;
    public StorageId StorageId => StorageInfo.StorageId;
    public long Size => StorageInfo.Size;
    public MimeType MimeType=> StorageInfo.MimeType;

    internal File(string name, StorageInfo storageInfo) : base(name)
    {        
        StorageInfo = storageInfo ?? throw new ArgumentNullException(nameof(storageInfo), "StorageInfo cannot be null.");
    }
    public bool IsEmpty => StorageInfo.IsEmpty;
    public void ApplyStorageInfo(StorageInfo storageInfo)
    {
        if (!storageInfo.IsValid)
            throw new InvalidOperationException("Invalid StorageInfo provided.");
        if (storageInfo.StorageId.IsEmpty)
            throw new InvalidOperationException("StorageId cannot be empty.");
        if (storageInfo.MimeType.IsEmpty)
            throw new InvalidOperationException("MimeType cannot be empty.");
        if (storageInfo.Size < 0)
            throw new InvalidOperationException("Size cannot be negative.");

        StorageInfo = storageInfo;
    }
}