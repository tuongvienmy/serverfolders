using Folders.Application.Abstractions;
using Folders.Application.DTOs;
using Folders.Application.Exceptions;
using MediatR;

namespace Folders.Application.UseCases.AddFileToFolder;
public class AddFileToFolderHandler : IRequestHandler<AddFileToFolderCommand, FileDto>
{
    private readonly IFolderRepository _folderRepo;
    private readonly IStorageManager _storageManager;
    public AddFileToFolderHandler(IFolderRepository folderRepo, IStorageManager storageManager)
    {
        _folderRepo = folderRepo;
        _storageManager = storageManager;
    }
    public async Task<FileDto> Handle(AddFileToFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepo.GetByIdAsync(request.FolderId); // retrieving folder aggreagete using the repository.
        if (folder is null)
            throw new FolderNotFoundException(request.FolderId);

        var fileName = request.FileName;
        // 1.Store the file data and get StorageInfo - StorageId, Size and MimeType.
        var storageInfo = await _storageManager.StoreStreamAsync(request.Data, request.StorageProviderKey);
        // 2.Add a file entity to the folder.
        var file = folder.AddFile(fileName, storageInfo);      // using domain logic from the core layer.
        // 3.Update folder.
        await _folderRepo.UpdateAsync(folder);                 // update folder using the repository
        // 5.Commit changes to the db.
        await _folderRepo.UnitOfWork.SaveChangesAsync(cancellationToken);  // Saving everything.

        return file.ToDto();         // return DTO to the client.
    }
}
