namespace Folders.Infrastructure.Storage.StorageProviders;

public class FileSystemStorageOptions
{
    public string BasePath { get; set; } = default!;
}

public class S3StorageOptions
{   
    public string BucketName { get; set; } = default!;
    public string? BucketPrefix { get; set; }
}

public class InMemoryStorageOptions
{
    public string Scope { get; set; } = "default";
}

