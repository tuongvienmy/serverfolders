using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.RenameFolder;

public class RenameFolderItemHandler : IRequestHandler<RenameFolderItemCommand, FolderItem>
{
    private readonly IFolderRepository _folderRepo;

    public RenameFolderItemHandler(IFolderRepository folderRepo)
    {
        _folderRepo = folderRepo;
    }
    public async Task<FolderItem> Handle(RenameFolderItemCommand request, CancellationToken cancellationToken)
    {
        var folderItem = await _folderRepo!.GetByIdAsync(request.FolderId);
        if (folderItem is null)
        {
            throw new FolderNotFoundException(request.FolderId);
        }

        folderItem.Rename(request.NewName);
        _folderRepo.UpdateAsync(folderItem);
        await _folderRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        return folderItem;
    }
}
