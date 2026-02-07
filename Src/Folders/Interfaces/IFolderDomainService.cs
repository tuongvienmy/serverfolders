using Folders.Core.Aggregates;
using Folders.Core.Values;


namespace Folders.Core.Interfaces;
public interface IFolderDomainService
{
    Task<Aggregates.File> AddFileAsync(Folder folder, string name, MimeType mimeType, Stream stream, StorageProviderKey storageProviderKey);
    Task<Aggregates.File> AddFileAsync(Folder folder, string name, MimeType mimeType, byte[] date, StorageProviderKey storageProviderKey);
    Task<Aggregates.File> MoveFileToAnotherStorageAsync(Aggregates.File file, StorageProviderKey storageProviderKey);
}
