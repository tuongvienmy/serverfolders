using MediatR;
using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using Folders.Application.DTOs;

namespace Folders.Application.UseCases.GetFileFromFolder;

public class GetFileFromFolderHandler : IRequestHandler<GetFileFromFolderCommand, GetFileFromFolderResult>
{
    private readonly IFolderRepository _folderRepo;
    private readonly IStorageManager _storageManager;

    public GetFileFromFolderHandler(IFolderRepository folderRepo, IStorageManager storageManager)
    {
        _folderRepo = folderRepo;
        _storageManager = storageManager;
    }

    public async Task<GetFileFromFolderResult> Handle(GetFileFromFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepo.GetByIdAsync(request.FolderId);
        if (folder is null)
            throw new FolderNotFoundException(request.FolderId);

        var file = folder.Items.OfType<Core.Aggregates.File>().FirstOrDefault(f => f.Id == request.FileId);

        if (file is null)
        {
            throw new KeyNotFoundException("File not found in the specified folder.");
        }

        if (request.IncludingData)
        {
            var data = await _storageManager.RetrieveAsync(file.StorageId);
            return new GetFileFromFolderResult(data, file.ToDto());
        }

        return new GetFileFromFolderResult(null, file.ToDto());

    }
}