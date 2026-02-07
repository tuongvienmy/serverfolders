namespace Folders.API.Models;

public record AddFolderRequest (Guid parentId, string FolderName);
