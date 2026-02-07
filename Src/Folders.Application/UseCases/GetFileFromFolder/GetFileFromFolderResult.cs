using File = Folders.Core.Aggregates.File;

namespace Folders.Application.UseCases.GetFileFromFolder;

public record GetFileFromFolderResult(byte[]? Data, File File);