namespace Folders.Core.Values;
public readonly struct StorageProviderKey : IEquatable<StorageProviderKey>
{
    public string Value { get; }

    private StorageProviderKey(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static readonly StorageProviderKey S3 = new("s3");
    public static readonly StorageProviderKey File = new("file");
    public static readonly StorageProviderKey Memory = new("mem");

    public static StorageProviderKey From(string key) => new(key);

    public override string ToString() => Value;

    public static implicit operator string(StorageProviderKey key) => key.Value;
    public static implicit operator StorageProviderKey(string value) => new(value);

    public bool Equals(StorageProviderKey other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is StorageProviderKey other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static bool operator ==(StorageProviderKey left, StorageProviderKey right) => left.Equals(right);
    public static bool operator !=(StorageProviderKey left, StorageProviderKey right) => !left.Equals(right);
}

