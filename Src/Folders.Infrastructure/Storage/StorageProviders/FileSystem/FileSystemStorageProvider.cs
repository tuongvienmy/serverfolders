using Folders.Core.Values;
using Folders.Application.Abstractions;

namespace Folders.Infrastructure.Storage.StorageProviders.FileSystem;
public class FileSystemStorageProvider: BaseStorageProvider
{      
    public FileSystemStorageProvider(string basePath, IStoragePathStrategy pathStrategy) : base(StorageProviderKey.File, basePath, pathStrategy)
    {
        Directory.CreateDirectory(Prefix);
    }

    public override async Task<StorageInfo> StoreAsync(byte[] data)
    {
        var storageId = GenerateStorageId();
        var filePath = storageId.CreateFilePath();

        var mimeType = MimeType.FromBuffer(data);

        await System.IO.File.WriteAllBytesAsync(filePath, data);

        return new StorageInfo(storageId,mimeType,data.LongLength);
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {       
        return await System.IO.File.ReadAllBytesAsync(storageId.RelativePath);
    }

    public override async Task<StorageInfo> StoreStreamAsync(Stream dataStream)
    {        
        var storageId = GenerateStorageId();
        var filePath = storageId.CreateFilePath();

        using var fileStream = System.IO.File.Create(filePath);
        await dataStream.CopyToAsync(fileStream);

        var mimeType = MimeType.FromStream(dataStream);
        return new StorageInfo(storageId,mimeType,fileStream.Length);
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {        
        var fullPath = Path.Combine(Prefix, storageId.RelativePath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await Task.FromResult(stream);
    }        

    public override Task DeleteAsync(StorageId storageId)
    {        
        if (System.IO.File.Exists(Path.Combine(Prefix, storageId.RelativePath)))
        { 
            System.IO.File.Delete(storageId.RelativePath);
        }
        return Task.CompletedTask;
    }    
}
