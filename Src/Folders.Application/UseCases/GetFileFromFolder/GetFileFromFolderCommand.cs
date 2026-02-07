using MediatR;
using Folders.Core.Aggregates;
using File = Folders.Core.Aggregates.File;

namespace Folders.Application.UseCases.GetFileFromFolder;

public record GetFileFromFolderCommand(Folder Folder, Guid FileId, bool IncludingData) : IRequest<GetFileFromFolderResult>;