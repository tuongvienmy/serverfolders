using Folders.Core.Values;
using MediatR;

namespace Folders.Application.UseCases.AddFileToFolder;
public record AddFileToFolderCommand(Core.Aggregates.Folder Folder, string FileName, byte[] Data, StorageProviderKey StorageProviderKey): IRequest<Core.Aggregates.File>;


