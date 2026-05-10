using DomainFundamentals;
using Folders.Core.Values;
using System.ComponentModel.DataAnnotations.Schema;

namespace Folders.Core.Aggregates;

public class Folder : FolderItem, IAggregateRoot
{
    protected readonly List<FolderItem> _items = new List<FolderItem>();

    private Folder(string name) : base(name)
    {
        ParentFolder = null;
    }

    public static Folder CreateRoot(string name) => new(name);

    public long NumberOfItems => _items.Count;

    [NotMapped]
    public IReadOnlyCollection<FolderItem> Items => _items;

    [NotMapped]
    public IReadOnlyCollection<File> Files => _items.OfType<File>().ToList();

    [NotMapped]
    public IReadOnlyCollection<Folder> SubFolders => _items.OfType<Folder>().ToList();

    /// <summary>
    /// Add a subfolder to this folder.
    /// </summary>
    public Folder AddFolder(string folderName)
    {
        var name = EnsureNameIsAvailable(folderName);

        var folder = new Folder(name)
        {
            ParentFolder = this,
            ParentFolderId = this.Id
        };

        _items.Add(folder);
        ModifiedAt = DateTime.UtcNow;
        return folder;
    }

    public File AddFile(string fileName, StorageInfo storageInfo)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        var file = new File(EnsureNameIsAvailable(fileName), storageInfo);        
        
        AttachItem(file);

        return file;
    }

    public FolderItem? Get(string name)
    {
        return _items.FirstOrDefault(i => i.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<FolderItem> FindAll(string nameFilter, bool partialMatch = false, Type? typeFilter = null)
    {
        bool Matches(FolderItem item) =>
            (partialMatch
                ? item.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
                : item.Name.Equals(nameFilter, StringComparison.OrdinalIgnoreCase))
            && (typeFilter == null || typeFilter.IsInstanceOfType(item));

        foreach (var item in _items)
        {
            if (Matches(item))
                yield return item;

            if (item is Folder subFolder)
            {
                foreach (var m in subFolder.FindAll(nameFilter, partialMatch, typeFilter))
                    yield return m;
            }
               
        }
    }

    /// <summary>
    /// Load initial set of items. Should only be called once.
    /// </summary>
    public void LoadItems(IEnumerable<FolderItem> items)
    {
        if (_items.Count > 0)
            throw new InvalidOperationException("LoadItems can only be called once, after creation.");

        foreach (var item in items)
        {
            AttachItem(item);
        }
    }

    public bool RemoveItem(string name)
    {
        var item = Get(name);
        if (item is null)
            return false;

        _items.Remove(item);
        item.ParentFolder = null;
        item.ParentFolderId = null;
        ModifiedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Gets the full path of this folder by traversing up the parent hierarchy.
    /// </summary>
    [NotMapped]
    public FolderPath Path
    {
        get
        {
            if (ParentFolder is null)
                return (FolderPath)Name;
            return ParentFolder.Path / Name;
        }
    }

    // Helpers

    internal string EnsureNameIsAvailable(string name)
    {
        var count = 1;
        var baseName = name;
        while (Get(baseName) is not null)
        {
            baseName = $"{name} ({count++})";
        }
        return baseName;
    }

    private void AttachItem(FolderItem file)
    {
        file.ParentFolder = this;
        file.ParentFolderId = this.Id;
        _items.Add(file);

        if (ModifiedAt == DateTime.MinValue)
            ModifiedAt = DateTime.UtcNow;
    }
}
