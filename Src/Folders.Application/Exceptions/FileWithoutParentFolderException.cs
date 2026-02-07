namespace Folders.Application.Exceptions;
public class FileWithoutParentFolderException: Exception
{
    public FileWithoutParentFolderException(Core.Aggregates.File file)
       : base($"File with ID '{file.Id}' does not have the Parent Folder.") { }
}
