using Folders.Application.DTOs;

namespace Folders.Application.UseCases.GetFolderByPath;

public record GetFolderByPathResult(FolderDto? Folder, bool Found);
