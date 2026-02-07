using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using MediatR;

namespace Folders.Application.UseCases.MoveFileToStorage;

public class MoveFileToStorageHandler : IRequestHandler<MoveFileToStorageCommand, Core.Aggregates.File>
{
    private readonly IStorageManager _storageManager;
    private readonly IFolderRepository _folderRepo;

    public MoveFileToStorageHandler(IStorageManager storageManager, IFolderRepository folderRepo)
    {
        _storageManager = storageManager;
        _folderRepo = folderRepo;
    }
    public async Task<Core.Aggregates.File> Handle(MoveFileToStorageCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var folder = file.ParentFolder;

        if (folder is null)
            throw new FileWithoutParentFolderException(file);

        var targetStorageProviderKey = request.StorageProviderKey;
        if (file.StorageId.Provider == targetStorageProviderKey)
            return file;

        var data = await _storageManager.RetrieveAsync(file.StorageId)
           .ContinueWith(async data =>
           {
               var storageInfo = await _storageManager.StoreAsync(data.Result, targetStorageProviderKey);
               await _storageManager.DeleteAsync(file.StorageId);
               file.ApplyStorageInfo(storageInfo);
           });

        _folderRepo.UpdateAsync(folder);                                  // Update the folder to reflect the change in file storage.
        await _folderRepo.UnitOfWork.SaveChangesAsync(cancellationToken); // Commit changes to the database.
        return file;
    }
}

