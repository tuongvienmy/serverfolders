using Folders.Core.Interfaces;
using Folders.Core.Aggregates;
using Folders.Core.Values;
using Folders.Application.Abstractions;


namespace Folders.Application.DomainServices;
public class FolderDomainService : IFolderDomainService
{
    private readonly IStorageManager _storageManager;

    public FolderDomainService(IStorageManager storageManager)
    {
        _storageManager = storageManager;
    }
    public async Task<Core.Aggregates.File> AddFileAsync(Folder folder, string name, MimeType mimeType, Stream stream, StorageProviderKey storageProviderKey)
    {
        folder.EnsureNameIsAvailable(name);
        long size = stream.CanSeek ? stream.Length : 0;
        var storageId = await _storageManager.StoreStreamAsync(stream, storageProviderKey);                    
        var file = Core.Aggregates.File.Create(name, mimeType, size, storageId);
        folder.AddFile(file);
        return file;
    }
    public async Task<Core.Aggregates.File> AddFileAsync(Folder folder, string name, MimeType mimeType, byte[] data, StorageProviderKey storageProviderKey)
    {
        folder.EnsureNameIsAvailable(name);        
        var storageId = await _storageManager.StoreAsync(data, storageProviderKey);
        var file = Core.Aggregates.File.Create(name, mimeType, data.LongLength, storageId);
        folder.AddFile(file);
        return file;
    }
    public async Task<Core.Aggregates.File> MoveFileToAnotherStorageAsync(Core.Aggregates.File file, StorageProviderKey targetStorageProviderKey)
    {
        if (file.StorageId.Provider == targetStorageProviderKey) 
            return file;

        var data = await _storageManager.RetrieveAsync(file.StorageId)
            .ContinueWith(async data =>
            {
                var newStorageId = await _storageManager.StoreAsync(data.Result, targetStorageProviderKey);
                await _storageManager.DeleteAsync(file.StorageId);
                file.StorageId = newStorageId;
            });
        return file;
    }
}
