using Folders.Application.Exceptions;
using Folders.Application.Abstractions;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.AddSubFolder;

public class AddSubFolderHandler : IRequestHandler<AddSubFolderCommand, Folder>
{
    private readonly IFolderRepository _folderRepo;    
    public AddSubFolderHandler(IFolderRepository folderRepo)
    {
        _folderRepo = folderRepo;        
    }
    public async Task<Folder> Handle(AddSubFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepo!.GetByIdAsync(request.parentFolderId) as Folder;
        if (folder is null)
        {
            throw new FolderNotFoundException(request.parentFolderId);
        }
        var subFolder = folder.AddFolder(request.subFolderName);
        _folderRepo.UpdateAsync(folder);
        await _folderRepo.UnitOfWork.SaveChangesAsync(cancellationToken);
        return subFolder;
    }
}

