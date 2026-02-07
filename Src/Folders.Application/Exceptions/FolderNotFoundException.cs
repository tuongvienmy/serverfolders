namespace Folders.Application.Exceptions;
public class FolderNotFoundException: Exception
{
    public FolderNotFoundException(Guid id)
        : base($"Folder with ID '{id}' was not found.") { }
}