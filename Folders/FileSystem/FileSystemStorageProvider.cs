namespace Folders.Core.FileSystem;
public class FileSystemStorageProvider: BaseStorageProvider
{      
    public FileSystemStorageProvider(string basePath): base(string.Empty, basePath)
    {
        Prefix = basePath;
        Directory.CreateDirectory(Prefix);
    }

    public override async Task<StorageId> StoreAsync(byte[] data)
    {
        var storageId = GenerateStorageId();
        var filePath = storageId.CreateFilePath();

        await System.IO.File.WriteAllBytesAsync(filePath, data);
        return filePath;
    }

    public override async Task<byte[]> RetrieveAsync(StorageId storageId)
    {       
        return await System.IO.File.ReadAllBytesAsync(storageId);
    }

    public override async Task<StorageId> StoreStreamAsync(Stream dataStream)
    {        
        var storageId = GenerateStorageId();
        var filePath = storageId.CreateFilePath();

        using var fileStream = System.IO.File.Create(filePath);
        await dataStream.CopyToAsync(fileStream);

        return filePath;
    }

    public override async Task<Stream> RetrieveStreamAsync(StorageId storageId)
    {        
        var memoryStream = new MemoryStream();
        await using var fileStream = System.IO.File.OpenRead(storageId);
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }        

    public override async Task DeleteAsync(StorageId storageId)
    {        
        if (System.IO.File.Exists(storageId))
        {
            System.IO.File.Delete(storageId);
        }
        return;
    }    
}
