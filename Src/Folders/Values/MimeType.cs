namespace Folders.Core.Values;

using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Text.RegularExpressions;

public readonly struct MimeType : IComparable, IEquatable<MimeType>
{
    private static readonly Regex MimeTypeRegex = new Regex(@"^[a-zA-Z0-9!#$&^_.+-]+/[a-zA-Z0-9!#$&^_.+-]+$", RegexOptions.Compiled);

    public string Value { get; }

    internal MimeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MIME type cannot be null or empty.", nameof(value));

        if (!MimeTypeRegex.IsMatch(value))
            throw new ArgumentException($"Invalid MIME type format: {value}", nameof(value));

        Value = value.ToLowerInvariant(); // Normalize
    }

    public static readonly MimeType Empty = new("application/x-empty");
    public bool IsEmpty => Value == "application/x-empty";

    public override string ToString() => Value;

    public static implicit operator string(MimeType mime) => mime.Value;
    public static implicit operator MimeType(string value) => new(value);

    public static MimeType FromFileName(string fileNameOrExtension)
    {
        var provider = new FileExtensionContentTypeProvider();

        if (!fileNameOrExtension.StartsWith(".") && !fileNameOrExtension.Contains("."))
            fileNameOrExtension = "." + fileNameOrExtension;

        if (provider.TryGetContentType(fileNameOrExtension, out var mime))
            return new MimeType(mime);

        return new MimeType("application/octet-stream"); // Fallback for unknown types
    }

    public static MimeType FromStream(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new InvalidOperationException("Stream is not readable.");

        var length = stream.Length > 256 ? 256 : stream.Length;

        byte[] buffer = new byte[length];

        stream.Seek(0, SeekOrigin.Begin); // Reset stream position

        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        stream.Seek(0, SeekOrigin.Begin); // Reset stream position

        if (bytesRead == 0)
            throw new InvalidOperationException("Stream is empty or could not be read.");

        string detectedMimeType = DetectMimeTypeFromBuffer(buffer);
        return new MimeType(detectedMimeType);
    }

    public static MimeType FromBuffer(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            throw new ArgumentException("Buffer cannot be null or empty.", nameof(buffer));
        string detectedMimeType = DetectMimeTypeFromBuffer(buffer);
        return new MimeType(detectedMimeType);
    }

    private static string DetectMimeTypeFromBuffer(byte[] buffer)
    {
        if (buffer.Length >= 4)
        {
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                return "image/png";

            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return "image/jpeg";

            if (buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46)
                return "application/pdf";

            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                return "image/gif";

            if (buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04)
                return "application/zip";
        }

        return "application/octet-stream"; // Fallback for unknown types
    }

    public static MimeType PlainText => new MimeType("text/plain");
    public static MimeType Json => new MimeType("application/json");
    public static MimeType Pdf => new MimeType("application/pdf");
    public static MimeType Png => new MimeType("image/png");
    public static MimeType Jpeg => new MimeType("image/jpeg");
    public int CompareTo(object? obj)
    {
        if (obj is MimeType other)
            return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        throw new ArgumentException("Object is not a MimeType.", nameof(obj));
    }
    public bool Equals(MimeType other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is MimeType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}