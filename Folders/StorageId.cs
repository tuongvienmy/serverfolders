namespace Folders.Core;
using System;

public readonly struct StorageId : IEquatable<StorageId>
{
    private readonly Uri _uri;

    /// <summary>
    /// Constructs a new StorageId from a provider key and a relative path.
    /// </summary>
    public StorageId(string provider, string path)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        // Format: provider://path (example: "s3://bucket/file.txt")
        var formatted = $"{provider.ToLowerInvariant()}://{path.TrimStart('/')}";
        _uri = new Uri(formatted, UriKind.Absolute);
    }

    private StorageId(Uri uri)
    {
        _uri = uri ?? throw new ArgumentNullException(nameof(uri));
    }

    /// <summary>
    /// Gets the provider key, e.g., "s3", "local", "azure".
    /// Used internally by the registry to resolve the storage provider.
    /// </summary>
    public string Provider => _uri.Scheme;

    /// <summary>
    /// Gets the path portion of the storage ID (relative within provider context).
    /// Only used internally by the provider implementation.
    /// </summary>
    public string Path => _uri.AbsolutePath.TrimStart('/');

    /// <summary>
    /// Gets the string value of this storage ID, e.g., "s3://bucket/file.txt".
    /// </summary>
    public string Value => _uri.ToString();

    /// <summary>
    /// Parses a storage ID from a string like "s3://bucket/path/to/file".
    /// </summary>
    public static StorageId Parse(string value)
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

    public bool Equals(StorageId other) => _uri.Equals(other._uri);
    public override bool Equals(object? obj) => obj is StorageId other && Equals(other);
    public override int GetHashCode() => _uri.GetHashCode();
}



