namespace Folders.Core;

using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;

public readonly struct MimeType: IComparable, IEquatable<MimeType>
{
    public string Value { get; }

    public MimeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MIME type cannot be null or empty.", nameof(value));

        Value = value.ToLowerInvariant(); // normalize
    }

    public override string ToString() => Value;

    public static implicit operator string(MimeType mime) => mime.Value;
    public static implicit operator MimeType(string value) => new(value);

    // Get from file extension (e.g., ".jpg", "file.png")
    public static MimeType FromFileName(string fileNameOrExtension)
    {
        var provider = new FileExtensionContentTypeProvider();

        if (!fileNameOrExtension.StartsWith(".") && !fileNameOrExtension.Contains("."))
            fileNameOrExtension = "." + fileNameOrExtension;

        if (provider.TryGetContentType(fileNameOrExtension, out var mime))
            return new MimeType(mime);

        return new MimeType("application/octet-stream"); // fallback
    }

    // Optional: Detect from stream (requires integration with a library like MimeDetective or custom logic)
    public static MimeType FromStream(Stream stream)
    {
        byte[] buffer = new byte[256];
        stream.Read(buffer, 0, buffer.Length);
        stream.Seek(0, SeekOrigin.Begin); // Reset stream position

        // Use a library or custom logic to detect MIME type
        string detectedMimeType = DetectMimeTypeFromBuffer(buffer);
        return new MimeType(detectedMimeType);
    }

    private static string DetectMimeTypeFromBuffer(byte[] buffer)
    {
        // Check for common file signatures (magic numbers)
        if (buffer.Length >= 4)
        {
            // PNG: 89 50 4E 47
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                return "image/png";

            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return "image/jpeg";

            // PDF: 25 50 44 46
            if (buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46)
                return "application/pdf";

            // GIF: 47 49 46 38
            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                return "image/gif";

            // ZIP (and Office formats like DOCX, XLSX): 50 4B 03 04
            if (buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04)
                return "application/zip";

            // MP4: 00 00 00 ?? 66 74 79 70
            if (buffer.Length >= 8 && buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70)
                return "video/mp4";
        }

        // Fallback to a default MIME type
        return "application/octet-stream";
    }

    public int CompareTo(object? obj)
    {
        return Value.CompareTo(obj is MimeType other ? other.Value : obj?.ToString() ?? string.Empty);
    }

    public bool Equals(MimeType other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is MimeType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    // Some common types (for convenience)
    public static MimeType Json => new("application/json");
    public static MimeType Pdf => new("application/pdf");
    public static MimeType Png => new("image/png");
    public static MimeType Jpeg => new("image/jpeg");
    public static MimeType PlainText => new("text/plain");
}
