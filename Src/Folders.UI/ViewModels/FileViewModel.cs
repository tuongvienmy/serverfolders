using Folders.Application.DTOs;
using Folders.Contracts;

namespace Folders.UI.ViewModels;

public class FileViewModel
{
    public FileViewModel(FileDto fileDto)
    {
        Id = fileDto.Id;
        Name = fileDto.Name;
        ParentFolderId = fileDto.ParentId;
        MimeType = fileDto.MimeType;
        StorageProvider = fileDto.StorageProviderKey;
        StorageId = fileDto.StorageId;
        SizeBytes = fileDto.Size;
        LastModified = fileDto.ModifiedAt;
    }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid? ParentFolderId { get; set; } = null;
    public string MimeType { get; set; } = "application/octet-stream";
    public string StorageProvider { get; set; } = "file"; // Local, S3, Azure, etc.
    public string StorageId { get; set; } = string.Empty;
    public long? SizeBytes { get; set; } = null;
    public DateTime? LastModified { get; set; } = null;
}
