using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.RenameFolder;
public record RenameFolderItemCommand(Guid FolderId, string NewName) : IRequest<FolderItem>;