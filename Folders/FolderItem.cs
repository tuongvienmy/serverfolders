namespace Folders.Core;
public abstract class FolderItem
{
    public string Name { get; protected set; }
    public IStorageProvider? StorageProvider { get; protected set; }
    public Folder? Parent { get; set; }
    public Dictionary<string, string> Metadata { get; protected set; } = [];

    public bool IsDeleted { get; protected set; } = false;
    public DateTime CreatedAt { get; protected set; }
    public DateTime ModifiedAt { get; protected set; }

    protected FolderItem(string name)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = CreatedAt;
    }

    public void Rename(string newName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        if (Parent != null && Parent.Items.Any(i => i.Name == newName))
            throw new InvalidOperationException("Name already exists in parent");
        Name = newName;
        ModifiedAt = DateTime.UtcNow;
    }
    public void SoftDelete() => IsDeleted = true;
    public void Restore() => IsDeleted = false;
    
}
