using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.AddSubFolder;
public record AddSubFolderCommand(Guid parentFolderId, string subFolderName) : IRequest<Folder>;

