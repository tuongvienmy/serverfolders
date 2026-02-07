using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using MediatR;

namespace Folders.Application.UseCases.AddFileToFolder;
public class AddFileToFolderHandler : IRequestHandler<AddFileToFolderCommand, Core.Aggregates.File>
{
    private readonly IFolderRepository _folderRepo;
    private readonly IStorageManager _storageManager;
    public AddFileToFolderHandler(IFolderRepository folderRepo, IStorageManager storageManager)
    {
        _folderRepo = folderRepo;
        _storageManager = storageManager;
    }
    public async Task<Core.Aggregates.File> Handle(AddFileToFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = request.Folder;
        var fileName = request.FileName;                                                                    // using domain logic from the domain service defined in the core layer.  
        // 1.Store the file data and get StorageInfo - StorageId, Size and MimeType.
        var storageInfo = await _storageManager.StoreAsync(request.Data, request.StorageProviderKey);       // using infrastructure logic from the abstraction defined in this layer.  
        // 2.Add a file entity to the folder.
        var file = folder.AddFile(fileName, storageInfo);                                                   // using domain logic from the core layer.                
        // 3.Update folder.
        await _folderRepo.UpdateAsync(folder);                                                              // using infrastructure logic from the abstraction defined in this layer.  
        // 5.Commit changes to the db.
        await _folderRepo.UnitOfWork.SaveChangesAsync(cancellationToken);                                   // using infrastructure logic from the abstraction defined in this layer.

        return file;
    }
}
