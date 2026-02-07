using Folders.Contracts;

namespace Folders.UI.ViewModels;

public class FolderViewModel
{
    public FolderViewModel(FolderDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        ParentId = dto.ParentId;
        IsExpanded = false;
        Folders = new List<FolderViewModel>();
        Files = new List<FileViewModel>();

        Folders.AddRange(dto.Items.OfType<FolderDto>().Select(f => new FolderViewModel(f)));
        Files.AddRange(dto.Items.OfType<FileDto>().Select(f => new FileViewModel(f)));        
    }
    public Guid Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public bool IsExpanded { get; set; }
    public List<FolderViewModel> Folders { get; set; } 
    public List<FileViewModel> Files { get; set; }
}


