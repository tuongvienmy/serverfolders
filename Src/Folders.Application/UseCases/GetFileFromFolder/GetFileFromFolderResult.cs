using Folders.Application.DTOs;

namespace Folders.Application.UseCases.GetFileFromFolder;

public record GetFileFromFolderResult(byte[]? Data, FileDto File);