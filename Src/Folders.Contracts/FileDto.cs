namespace Folders.Contracts;

public record FileDto(Guid Id, string Name, Guid? ParentId, DateTime CreatedAt, DateTime ModifiedAt, string StorageProviderKey, string StorageId, string MimeType, long Size) : FolderItemDto(Id, Name, ParentId, CreatedAt, ModifiedAt, "file");