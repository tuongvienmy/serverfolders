namespace Folders.Core;

using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;

public readonly struct MimeType
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
        // Placeholder logic — always returns binary
        // Replace with actual MIME detection if needed
        return new MimeType("application/octet-stream");
    }

    // Some common types (for convenience)
    public static MimeType Json => new("application/json");
    public static MimeType Pdf => new("application/pdf");
    public static MimeType Png => new("image/png");
    public static MimeType Jpeg => new("image/jpeg");
    public static MimeType PlainText => new("text/plain");
}
