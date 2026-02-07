namespace Folders.API.Models;

public record AddFileRequest(IFormFile File, string StorageProviderKey);