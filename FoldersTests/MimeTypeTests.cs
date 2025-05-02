using Folders.Core;

namespace Folders.Core;

[TestClass]
public class MimeTypeTests
{
    [TestMethod]
    public void MimeType_Constructor_ShouldSetValue()
    {
        // Arrange
        string mimeTypeValue = "application/json";

        // Act
        var mimeType = new MimeType(mimeTypeValue);

        // Assert
        Assert.AreEqual(mimeTypeValue, mimeType.Value);
    }

    [TestMethod]
    public void MimeType_FromFileName_ShouldReturnCorrectMimeType()
    {
        // Arrange
        string fileName = "example.png";

        // Act
        var mimeType = MimeType.FromFileName(fileName);

        // Assert
        Assert.AreEqual("image/png", mimeType.Value);
    }

    [TestMethod]
    public void MimeType_FromFileName_ShouldHandleUnknownExtension()
    {
        // Arrange
        string fileName = "example.unknown";

        // Act
        var mimeType = MimeType.FromFileName(fileName);

        // Assert
        Assert.AreEqual("application/octet-stream", mimeType.Value); // Default for unknown types
    }

    [TestMethod]
    public void MimeType_FromStream_ShouldDetectMimeType()
    {
        // Arrange
        byte[] pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG file header
        using var stream = new MemoryStream(pngHeader);

        // Act
        var mimeType = MimeType.FromStream(stream);

        // Assert
        Assert.AreEqual("image/png", mimeType.Value);
    }

    [TestMethod]
    public void MimeType_CommonTypes_ShouldReturnExpectedValues()
    {
        // Assert
        Assert.AreEqual("application/json", MimeType.Json.Value);
        Assert.AreEqual("application/pdf", MimeType.Pdf.Value);
        Assert.AreEqual("image/png", MimeType.Png.Value);
        Assert.AreEqual("image/jpeg", MimeType.Jpeg.Value);
        Assert.AreEqual("text/plain", MimeType.PlainText.Value);
    }

    [TestMethod]
    public void MimeType_ToString_ShouldReturnValue()
    {
        // Arrange
        var mimeType = new MimeType("application/xml");

        // Act
        string result = mimeType.ToString();

        // Assert
        Assert.AreEqual("application/xml", result);
    }
}
