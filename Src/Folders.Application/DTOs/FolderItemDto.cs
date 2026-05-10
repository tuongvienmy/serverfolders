namespace Folders.Application.DTOs;

public abstract record FolderItemDto(Guid Id, string Name, Guid? ParentId, DateTime CreatedAt, DateTime ModifiedAt, string Type);