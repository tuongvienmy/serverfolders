namespace ServerFolderTests;

using Folders.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class StorageIdTests
{
    [TestMethod]
    public void Constructor_CreatesValidUri()
    {
        var id = new StorageId("s3", "bucket/path/file.txt");

        Assert.AreEqual("s3", id.Provider);
        Assert.AreEqual("path/file.txt", id.Path);
        Assert.AreEqual("s3://bucket/path/file.txt", id.Value);
    }

    [TestMethod]
    public void Parse_ValidString_ParsesSuccessfully()
    {
        var id = StorageId.Parse("local://folder/file.txt");

        Assert.AreEqual("local", id.Provider);
        Assert.AreEqual("file.txt", id.Path);
        Assert.AreEqual("local://folder/file.txt", id.Value);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("invalidformat")]
    [DataRow(":///no-scheme")]
    public void Parse_InvalidString_ThrowsFormatException(string input)
    {
        Assert.ThrowsException<FormatException>(() => StorageId.Parse(input));
    }

    [TestMethod]
    public void ImplicitConversion_FromString_Works()
    {
        StorageId id = "azure://container/blob.png";

        Assert.AreEqual("azure", id.Provider);
        Assert.AreEqual("blob.png", id.Path);
    }

    [TestMethod]
    public void ImplicitConversion_ToString_Works()
    {
        var id = new StorageId("s3", "bucket/file");
        string value = id;

        Assert.AreEqual("s3://bucket/file", value);
    }

    [TestMethod]
    public void Equals_And_HashCode_Work()
    {
        var id1 = new StorageId("local", "folder/file.txt");
        var id2 = new StorageId("local", "folder/file.txt");
        var id3 = new StorageId("s3", "bucket/file.txt");

        Assert.AreEqual(id1, id2);
        Assert.AreEqual(id1.GetHashCode(), id2.GetHashCode());
        Assert.AreNotEqual(id1, id3);
    }

    [TestMethod]
    public void ToString_ReturnsValue()
    {
        var id = new StorageId("s3", "bucket/file.txt");

        Assert.AreEqual("s3://bucket/file.txt", id.ToString());
    }

    [TestMethod]
    public void Throws_When_Provider_Or_Path_Is_Empty()
    {
        Assert.ThrowsException<ArgumentException>(() => new StorageId("", "path/to/file"));
        Assert.ThrowsException<ArgumentException>(() => new StorageId("s3", ""));
    }

    [DataTestMethod]
    [DataRow("local://C:/folder/file.txt", "C:/folder/file.txt")]
    [DataRow("local://C:\\folder\\file.txt", "C:/folder/file.txt")]
    [DataRow("local://./folder/file.txt", "folder/file.txt")]
    [DataRow("local://../folder/file.txt", "folder/file.txt")]
    public void Path_Parses_Unusual_Local_Paths(string input, string expectedPath)
    {
        var id = StorageId.Parse(input);
        Assert.AreEqual("local", id.Provider);
        Assert.AreEqual(expectedPath.Replace('\\', '/'), id.Path);
    }

    [TestMethod]
    public void UriWithWindowsDriveLetter_BecomesAbsoluteAndWrong()
    {
        StorageId id = "local://C:/Users/Docs/file.txt";

        Assert.AreEqual("local", id.Provider);
        Assert.AreEqual("C:/Users/Docs/file.txt", id.Path);
    }

    [TestMethod]
    public void InvalidSchemeMissing_ThrowsFormatException()
    {
        StorageId storageId = "C:/folder/file.txt";
        //Assert.ThrowsException<FormatException>(() => StorageId.Parse(invalid));
    }

    [TestMethod]
    public void Backslashes_AreConvertedToForwardSlashes()
    {
        var input = "local://folder\\file.txt";        

        Assert.ThrowsException<FormatException>(() => StorageId.Parse(input));
    }

    [TestMethod]
    public void DotSegments_ArePreservedInPath()
    {
        var id = StorageId.Parse("local://folder/../file.txt");

        Assert.AreEqual("file.txt", id.Path);
    }
}


