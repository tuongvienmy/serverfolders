using System;

namespace Folders.Core.Values;


/// <summary>
/// A storage ID is a unique identifier for a file in any storage proivder.
/// </summary>
/// <remarks>
/// The client code can be used interchangeably as a string. The client code should not care about the internal structure of the id - it receives it from a storage provider when it stores a file 
/// and uses it to retrieve the file later.
/// The client code cannot create a StorageId directly. It must be created by a storage provider.
/// The client code cannot inspect the internal structure of the StorageId.
/// </remarks> 
public readonly struct StorageId : IEquatable<StorageId>
{
    private readonly Uri _uri;

    /// <summary>
    /// Constructs a new StorageId from a provider key and a relative path.
    /// </summary>
    public StorageId(StorageProviderKey provider, string prefix, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        var prefixPart = string.IsNullOrEmpty(prefix) ? "" : $"//{prefix}/";
        _uri = new Uri($"{provider}:{prefixPart}{relativePath.TrimStart("/")}".Replace("\\", "/"));        
    }

    public StorageId(Uri uri)
    {
        _uri = uri ?? throw new ArgumentNullException(nameof(uri));
    } 
    
    public StorageId(string uriString): this(new Uri(uriString, UriKind.Absolute)){}

    /// <summary>
    /// Gets the provider key, e.g., "s3", "local", "azure".
    /// Used internally by the registry to resolve the storage provider.
    /// </summary>
    public StorageProviderKey Provider => _uri.Scheme;

    /// <summary>
    /// Gets the path portion of the storage ID (relative within provider context).
    /// Only used internally by the provider implementation.
    /// </summary>
    public string RelativePath
    {
        get
        {
            var path = _uri.AbsolutePath.TrimStart('/');
            return Uri.UnescapeDataString(path);
        }
    }

    public string Prefix => _uri.Authority; // The bucket name, base folder, etc.    

    /// <summary>
    /// Gets the string value of this storage ID, e.g., "s3://bucket/file.txt".
    /// </summary>
    public string Value => Uri.UnescapeDataString(_uri.ToString());

    public static readonly StorageId Empty = new StorageId(new Uri("empty://", UriKind.Absolute));

    public bool IsEmpty => _uri.Scheme.Equals("empty", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Parses a storage ID from a string like "s3://bucket/path/to/file".
    /// </summary>
    internal static StorageId Parse(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new FormatException($"Invalid StorageId format: '{value}'");

        return new StorageId(uri);
    }

    /// <summary>
    /// Implicitly converts a string to a StorageId by parsing it.
    /// </summary>
    public static implicit operator StorageId(string value) => Parse(value);

    /// <summary>
    /// Implicitly converts a StorageId to its string value.
    /// </summary>
    public static implicit operator string(StorageId id) => id.Value;

    public override string ToString() => Value;

    public bool Equals(StorageId other) => Uri.Compare(_uri, other._uri, UriComponents.AbsoluteUri,
        UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
    public override bool Equals(object? obj) => obj is StorageId other && Equals(other);
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_uri.ToString());
}



