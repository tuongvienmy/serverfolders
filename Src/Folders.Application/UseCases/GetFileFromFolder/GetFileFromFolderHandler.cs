using MediatR;
using Folders.Application.Abstractions;

namespace Folders.Application.UseCases.GetFileFromFolder;

public class GetFileFromFolderHandler : IRequestHandler<GetFileFromFolderCommand, GetFileFromFolderResult>
{
    private readonly IStorageManager _storageManager;

    public GetFileFromFolderHandler(IStorageManager storageManager)
    {
        _storageManager = storageManager;
    }

    public async Task<GetFileFromFolderResult> Handle(GetFileFromFolderCommand request, CancellationToken cancellationToken)
    {
        var file = request.Folder.Items.OfType<Core.Aggregates.File>().FirstOrDefault(f => f.Id == request.FileId);

        if (file is null)
        {
            throw new KeyNotFoundException("File not found in the specified folder.");
        }

        if (request.IncludingData)
        {
            var data = await _storageManager.RetrieveAsync(file.StorageId);
            return new GetFileFromFolderResult(data, file);
        }

        return new GetFileFromFolderResult(null, file);

    }
}