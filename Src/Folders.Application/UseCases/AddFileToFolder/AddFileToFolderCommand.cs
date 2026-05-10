using Folders.Application.DTOs;
using Folders.Core.Values;
using MediatR;

namespace Folders.Application.UseCases.AddFileToFolder;
public record AddFileToFolderCommand(Guid FolderId, string FileName, Stream Data, StorageProviderKey StorageProviderKey): IRequest<FileDto>;


