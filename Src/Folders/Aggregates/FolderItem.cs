using DomainFundamentals;

namespace Folders.Core.Aggregates;
public abstract class FolderItem: Entity
{     
    protected FolderItem(string name)
        :base()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = CreatedAt;
        IsDeleted = false;
    }

    public string Name { get; set; }    
    public Folder? ParentFolder { get; set; }
    public Guid? ParentFolderId { get; set; }  // Foreign key for the parent FolderItem 
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string,string>();
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }    

    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        if (ParentFolder is not null && ParentFolder.Items.Any(i => i.Name == newName))
            throw new InvalidOperationException("Name already exists in parent");
        
        var oldName = Name;
        Name = newName;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new Events.FolderItemRenameDomainEvent(Id, Name, newName));
    }
    public void SoftDelete() => UpdateIsDeleted(true);
    public void Restore() => UpdateIsDeleted(false);

    private void UpdateIsDeleted(bool isDeleted)
    {
        if (IsDeleted != isDeleted)
        {
            IsDeleted = isDeleted;
            ModifiedAt = DateTime.UtcNow;
            if (isDeleted)
                AddDomainEvent(new Events.FolderItemSoftDeletedDomainEvent(Id));
            else
                AddDomainEvent(new Events.FolderItemRestoredDomainEvent(Id));
        }
    }

}
