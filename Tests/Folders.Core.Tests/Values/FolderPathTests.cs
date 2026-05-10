using Folders.Core.Values;

namespace Folders.Core.Tests.Values;

[TestClass]
public sealed class FolderPathTests
{
    [TestMethod]
    public void FolderPath_ImplicitConversion_FromString_CreatesPath()
    {
        // Arrange
        string pathString = "/root/folder1/folder2";

        // Act
        FolderPath path = pathString;

        // Assert
        Assert.IsNotNull(path);
        Assert.AreEqual(3, path.Segments.Count);
        Assert.AreEqual("root", path.Segments[0]);
        Assert.AreEqual("folder1", path.Segments[1]);
        Assert.AreEqual("folder2", path.Segments[2]);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_FromString_WithoutLeadingSlash_CreatesPath()
    {
        // Arrange
        string pathString = "root/folder1/folder2";

        // Act
        FolderPath path = pathString;

        // Assert
        Assert.IsNotNull(path);
        Assert.AreEqual(3, path.Segments.Count);
        Assert.AreEqual("root", path.Segments[0]);
        Assert.AreEqual("folder1", path.Segments[1]);
        Assert.AreEqual("folder2", path.Segments[2]);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_FromString_SingleSegment_CreatesPath()
    {
        // Arrange
        string pathString = "/root";

        // Act
        FolderPath path = pathString;

        // Assert
        Assert.IsNotNull(path);
        Assert.AreEqual(1, path.Segments.Count);
        Assert.AreEqual("root", path.Segments[0]);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_ToString_ReturnsPathString()
    {
        // Arrange
        FolderPath path = "/root/folder1/folder2";

        // Act
        string pathString = path;

        // Assert
        Assert.AreEqual("/root/folder1/folder2", pathString);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_RoundTrip_StringToPathToString()
    {
        // Arrange
        string originalString = "/root/folder1/folder2";

        // Act
        FolderPath path = originalString;
        string result = path;

        // Assert
        Assert.AreEqual(originalString, result);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_FromString_NullOrEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        try
        {
            FolderPath path = (string)null!;
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_FromString_Whitespace_ThrowsArgumentException()
    {
        // Act & Assert
        try
        {
            FolderPath path = "   ";
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_ToString_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        FolderPath? path = null;

        // Act & Assert
        try
        {
            string result = path!;
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void FolderPath_DivideOperator_FolderPathWithString_ConcatenatesPaths()
    {
        // Arrange
        FolderPath path = "/root";

        // Act
        FolderPath result = path / "folder1";

        // Assert
        Assert.AreEqual(2, result.Segments.Count);
        Assert.AreEqual("root", result.Segments[0]);
        Assert.AreEqual("folder1", result.Segments[1]);
        Assert.AreEqual("/root/folder1", result.ToString());
    }

    [TestMethod]
    public void FolderPath_DivideOperator_MultipleSegments_BuildsCorrectPath()
    {
        // Arrange
        FolderPath path = "/root";

        // Act
        FolderPath result = path / "folder1" / "folder2" / "folder3";

        // Assert
        Assert.AreEqual(4, result.Segments.Count);
        Assert.AreEqual("root", result.Segments[0]);
        Assert.AreEqual("folder1", result.Segments[1]);
        Assert.AreEqual("folder2", result.Segments[2]);
        Assert.AreEqual("folder3", result.Segments[3]);
        Assert.AreEqual("/root/folder1/folder2/folder3", result.ToString());
    }

    [TestMethod]
    public void FolderPath_DivideOperator_TwoFolderPaths_ConcatenatesPaths()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/folder2/folder3";

        // Act
        FolderPath result = path1 / path2;

        // Assert
        Assert.AreEqual(4, result.Segments.Count);
        Assert.AreEqual("root", result.Segments[0]);
        Assert.AreEqual("folder1", result.Segments[1]);
        Assert.AreEqual("folder2", result.Segments[2]);
        Assert.AreEqual("folder3", result.Segments[3]);
        Assert.AreEqual("/root/folder1/folder2/folder3", result.ToString());
    }

    [TestMethod]
    public void FolderPath_PlusOperator_FolderPathWithString_ConcatenatesPaths()
    {
        // Arrange
        FolderPath path = "/root";

        // Act
        FolderPath result = path + "folder1";

        // Assert
        Assert.AreEqual(2, result.Segments.Count);
        Assert.AreEqual("root", result.Segments[0]);
        Assert.AreEqual("folder1", result.Segments[1]);
        Assert.AreEqual("/root/folder1", result.ToString());
    }

    [TestMethod]
    public void FolderPath_PlusOperator_StringWithFolderPath_ConcatenatesPaths()
    {
        // Arrange
        FolderPath pathPart = "/folder2/folder3";

        // Act
        FolderPath result = "/root/folder1" + pathPart;

        // Assert
        Assert.AreEqual(4, result.Segments.Count);
        Assert.AreEqual("root", result.Segments[0]);
        Assert.AreEqual("folder1", result.Segments[1]);
        Assert.AreEqual("folder2", result.Segments[2]);
        Assert.AreEqual("folder3", result.Segments[3]);
        Assert.AreEqual("/root/folder1/folder2/folder3", result.ToString());
    }

    [TestMethod]
    public void FolderPath_PlusOperator_MultipleSegments_BuildsCorrectPath()
    {
        // Arrange
        FolderPath path = "/root";

        // Act
        FolderPath result = path + "folder1" + "folder2" + "folder3";

        // Assert
        Assert.AreEqual(4, result.Segments.Count);
        Assert.AreEqual("/root/folder1/folder2/folder3", result.ToString());
    }

    [TestMethod]
    public void FolderPath_PlusOperator_TwoFolderPaths_ConcatenatesPaths()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/folder2/folder3";

        // Act
        FolderPath result = path1 + path2;

        // Assert
        Assert.AreEqual(4, result.Segments.Count);
        Assert.AreEqual("/root/folder1/folder2/folder3", result.ToString());
    }

    [TestMethod]
    public void FolderPath_Equality_SameSegments_AreEqual()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/root/folder1";

        // Assert
        Assert.AreEqual(path1, path2);
    }

    [TestMethod]
    public void FolderPath_Equality_DifferentSegments_AreNotEqual()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/root/folder2";

        // Assert
        Assert.AreNotEqual(path1, path2);
    }

    [TestMethod]
    public void FolderPath_Equality_DifferentDepth_AreNotEqual()
    {
        // Arrange
        FolderPath path1 = "/root";
        FolderPath path2 = "/root/folder1";

        // Assert
        Assert.AreNotEqual(path1, path2);
    }

    [TestMethod]
    public void FolderPath_Segments_IsReadOnly()
    {
        // Arrange
        FolderPath path = "/root/folder1";

        // Assert
        Assert.IsNotNull(path.Segments);
        Assert.IsInstanceOfType(path.Segments, typeof(IReadOnlyList<string>));
    }

    [TestMethod]
    public void FolderPath_GetHashCode_SameSegments_SameHash()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/root/folder1";

        // Assert
        Assert.AreEqual(path1.GetHashCode(), path2.GetHashCode());
    }

    [TestMethod]
    public void FolderPath_GetHashCode_DifferentSegments_DifferentHash()
    {
        // Arrange
        FolderPath path1 = "/root/folder1";
        FolderPath path2 = "/root/folder2";

        // Assert
        Assert.AreNotEqual(path1.GetHashCode(), path2.GetHashCode());
    }

    [TestMethod]
    public void FolderPath_DivideOperator_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        FolderPath? path = null;

        // Act & Assert
        try
        {
            FolderPath result = path! / "segment";
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void FolderPath_PlusOperator_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        FolderPath? path = null;

        // Act & Assert
        try
        {
            FolderPath result = path! + "segment";
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }
}
