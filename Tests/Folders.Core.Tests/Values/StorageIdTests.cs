using Folders.Core.Values;

namespace Folders.Core.Tests.Values;

[TestClass]
public class StorageIdTests
{
    [TestMethod]
    public void StorageId_Constructor_ShouldCreateValidId_WhenProviderKey_And_Path_NotNull()
    {
        // Arrange        
        var id = new StorageId("s3", "bucket", "/path/file.txt");

        Assert.IsFalse(id.IsEmpty);
        Assert.AreEqual<StorageProviderKey>("s3", id.Provider);
        Assert.AreEqual("path/file.txt", id.RelativePath);
        Assert.AreEqual("s3://bucket/path/file.txt", id.Value);
    }
    [TestMethod]
    public void StorageId_Constructor_ShouldCreateValidId_WithRandomProviderKey_And_Path()
    {
        // Arrange        
        var id = new StorageId("unknown", "host", "/path/file.txt");

        Assert.IsFalse(id.IsEmpty);
        Assert.AreEqual<StorageProviderKey>("unknown", id.Provider);
        Assert.AreEqual("path/file.txt", id.RelativePath);
        Assert.AreEqual("unknown://host/path/file.txt", id.Value);
    }

    [TestMethod]
    public void StorageId_Parse_ShouldParseSucessfully_WithUriString()
    {
        var id = StorageId.Parse("s3://bucket/path/to/file.txt");
        Assert.IsFalse(id.IsEmpty);
        Assert.AreEqual<StorageProviderKey>("s3", id.Provider);
        Assert.AreEqual("path/to/file.txt", id.RelativePath);
    }
    [TestMethod]
    [DataRow("InvalidUri")]
    [DataRow("")]
    [DataRow("null")]
    [DataRow("://bucket/")]
    public void StorageId_Parse_ShouldThrow_WhenInvalidUri(string uri)
    {
        Assert.ThrowsExactly<FormatException>(() => StorageId.Parse(uri));
    }

    [TestMethod]
    public void StorageId_Parse_ShouldParseSucessfully_WithFileSystemPathname()
    {
        var id = StorageId.Parse(@"d:\folder\file.txt");
        Assert.IsFalse(id.IsEmpty);
        Assert.AreEqual<StorageProviderKey>("file", id.Provider);
        Assert.AreEqual(@"d:/folder/file.txt", id.RelativePath);
        Assert.AreEqual(@"file:///d:/folder/file.txt", id.Value);
    }
}
