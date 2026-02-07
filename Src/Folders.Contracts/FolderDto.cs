namespace Folders.Contracts;

public record FolderDto(Guid Id, string Name, Guid? ParentId, DateTime CreatedAt, DateTime ModifiedAt, IReadOnlyList<FolderItemDto> Items) : FolderItemDto(Id, Name, ParentId, CreatedAt, ModifiedAt, "folder");
