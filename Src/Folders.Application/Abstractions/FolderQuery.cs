namespace Folders.Application.Abstractions;

public class FolderQuery
{
    public string? Name { get; set; }
    public bool RootsOnly { get; set; }    
    public bool ExactMatch { get; set; }   
    public bool IncludeSubFolders { get; set; } = true;
}
