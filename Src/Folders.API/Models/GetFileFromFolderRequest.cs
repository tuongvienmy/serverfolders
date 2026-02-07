namespace Folders.API.Models;

public record GetFileFromFolderRequest(Guid FolderId, Guid FileId);
