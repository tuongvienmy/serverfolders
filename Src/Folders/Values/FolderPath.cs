using DomainFundamentals;

namespace Folders.Core.Values;

public sealed class FolderPath : ValueObject
{
    public IReadOnlyList<string> Segments { get; }

    private FolderPath(IEnumerable<string> segments)
    {
        Segments = segments.ToList().AsReadOnly();
    }

    public override string ToString() => "/" + string.Join("/", Segments);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var s in Segments)
            yield return s;
    }

    /// <summary>
    /// Implicit conversion from string path to FolderPath.
    /// </summary>
    public static implicit operator FolderPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (segments.Count == 0)
            throw new ArgumentException("Path must contain at least one segment.", nameof(path));

        return new FolderPath(segments);
    }

    /// <summary>
    /// Implicit conversion from FolderPath to string.
    /// </summary>
    public static implicit operator string(FolderPath path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        return path.ToString();
    }

    /// <summary>
    /// Concatenates a FolderPath with a segment using the / operator.
    /// </summary>
    public static FolderPath operator /(FolderPath path, string segment)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment cannot be null or empty.", nameof(segment));

        var newSegments = path.Segments.Concat([segment]).ToList();
        return new FolderPath(newSegments);
    }

    /// <summary>
    /// Concatenates two FolderPaths using the / operator.
    /// </summary>
    public static FolderPath operator /(FolderPath path1, FolderPath path2)
    {
        if (path1 == null)
            throw new ArgumentNullException(nameof(path1));

        if (path2 == null)
            throw new ArgumentNullException(nameof(path2));

        var newSegments = path1.Segments.Concat(path2.Segments).ToList();
        return new FolderPath(newSegments);
    }

    /// <summary>
    /// Concatenates a FolderPath with a segment using the + operator.
    /// </summary>
    public static FolderPath operator +(FolderPath path, string segment)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment cannot be null or empty.", nameof(segment));

        var newSegments = path.Segments.Concat([segment]).ToList();
        return new FolderPath(newSegments);
    }

    /// <summary>
    /// Concatenates a string path with a FolderPath using the + operator.
    /// </summary>
    public static FolderPath operator +(string path, FolderPath folderPath)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (folderPath == null)
            throw new ArgumentNullException(nameof(folderPath));

        FolderPath basePath = path;
        var newSegments = basePath.Segments.Concat(folderPath.Segments).ToList();
        return new FolderPath(newSegments);
    }

    /// <summary>
    /// Concatenates two FolderPaths using the + operator.
    /// </summary>
    public static FolderPath operator +(FolderPath path1, FolderPath path2)
    {
        if (path1 == null)
            throw new ArgumentNullException(nameof(path1));

        if (path2 == null)
            throw new ArgumentNullException(nameof(path2));

        var newSegments = path1.Segments.Concat(path2.Segments).ToList();
        return new FolderPath(newSegments);
    }
}
