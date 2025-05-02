namespace Folders.Core;

public class Folder : FolderItem
{
    protected Dictionary<string, FolderItem> _items = [];
    private Folder(string name) : base(name)  
    {        
        Parent = null;
    }
    public long NumberOfItems => _items.Count;    
    public IReadOnlyCollection <FolderItem> Items => _items.Values;
    public IReadOnlyCollection<File> Files => [.. _items.Values.OfType<File>()];
    public IReadOnlyCollection<Folder> SubFolders => [.. _items.Values.OfType<Folder>()];    

    public static Folder NewFolder(string name) => new(name);
    
    /// <summary>
    /// Add a sub folder
    /// </summary>
    /// <param name="folderName">Name of the sub folder</param>
    public Folder AddFolder(string folderName)
    {
        if (_items.ContainsKey(folderName))
            throw new InvalidOperationException($"An folder named '{folderName}' already exists.");

        var folder = new Folder(folderName) {  Parent = this };
        _items[folderName] = folder;
        
        ModifiedAt = DateTime.UtcNow;

        return folder;
    }    
    public File AddFile(string name, MimeType mimeType, Stream dataStream, IStorageProvider storageProvider)
    {
        if (_items.ContainsKey(name))
            throw new InvalidOperationException($"A file named '{name}' already exists.");
        var file = File.Add(name, mimeType, dataStream, storageProvider);
        file.Parent = this;
        _items[name] = file;
        ModifiedAt = DateTime.UtcNow;
        return file;
    }
    public File AddFile(string name, MimeType mimeType, byte[] data, IStorageProvider storageProvider)
    {
        if (_items.ContainsKey(name))
            throw new InvalidOperationException($"A file named '{name}' already exists.");
        var file = File.Add(name, mimeType, data, storageProvider);
        file.Parent = this;
        _items[name] = file;
        ModifiedAt = DateTime.UtcNow;
        return file;
    }
    public File AddFile(FileInfo fileInfo, IStorageProvider storageProvider)
    {
        if (_items.ContainsKey(fileInfo.Name))
            throw new InvalidOperationException($"A file named '{fileInfo.Name}' already exists.");
        var file = File.Add(fileInfo, storageProvider);
        file.Parent = this;
        _items[fileInfo.Name] = file;
        ModifiedAt = DateTime.UtcNow;
        return file;
    }

    public FolderItem? Get(string name)
    {
        _items.TryGetValue(name, out FolderItem? value);
        return value;
    }
    public IEnumerable<FolderItem> FindAll(string nameFilter, bool partialMatch = false, Type? typeFilter = null)
    {
        foreach (var item in Items)
        {
            bool nameMatches = partialMatch
                ? item.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
                : item.Name.Equals(nameFilter, StringComparison.OrdinalIgnoreCase);

            bool typeMatches = typeFilter == null || typeFilter.IsInstanceOfType(item);

            if (nameMatches && typeMatches)
                yield return item;

            if (item is Folder subFolder)
            {
                foreach (var match in subFolder.FindAll(nameFilter, partialMatch, typeFilter))
                    yield return match;
            }
        }
    }

}

