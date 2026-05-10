namespace Folders.Core.Values;
public record StorageInfo
{
    public StorageId StorageId { get; private set; }
    public MimeType MimeType { get; private set; }
    public long Size { get; private set; }

    public StorageInfo(StorageId storageId, MimeType mimeType, long size)
    {
        StorageId = storageId;
        MimeType = mimeType;
        Size = size;
    }

    public static StorageInfo Empty => new(StorageId.Empty, MimeType.Empty, 0);
    public bool IsEmpty => StorageId.IsEmpty && MimeType.IsEmpty && Size == 0;
    public bool IsValid => !StorageId.IsEmpty && !MimeType.IsEmpty && Size >= 0;
    public override string ToString() => $"{StorageId} ({MimeType}, {Size} bytes)";
}
