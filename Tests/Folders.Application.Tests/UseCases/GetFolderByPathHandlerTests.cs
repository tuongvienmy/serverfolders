using Folders.Application.Abstractions;
using Folders.Application.DTOs;
using Folders.Application.UseCases.GetFolderByPath;
using Folders.Core.Aggregates;
using NSubstitute;

namespace Folders.Application.Tests.UseCases;

[TestClass]
public sealed class GetFolderByPathHandlerTests
{
    private IFolderRepository? _folderRepository;
    private GetFolderByPathHandler? _handler;

    [TestInitialize]
    public void Initialize()
    {
        _folderRepository = Substitute.For<IFolderRepository>();
        _handler = new GetFolderByPathHandler(_folderRepository);
    }

    [TestMethod]
    public async Task GetFolderByPath_RootFolderExists_ReturnsFolder()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("root", result.Folder.Name);
    }

    [TestMethod]
    public async Task GetFolderByPath_RootFolderDoesNotExist_ReturnsFolderNotFound()
    {
        // Arrange
        _folderRepository!.GetSubtreeWithHierarchyAsync("nonexistent")
            .Returns((Folder?)null);

        var query = new GetFolderByPathQuery("/nonexistent");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Found);
        Assert.IsNull(result.Folder);
    }

    [TestMethod]
    public async Task GetFolderByPath_NestedFolderExists_ReturnsFolder()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");
        var subFolder = rootFolder.AddFolder("subfolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root/subfolder");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("subfolder", result.Folder.Name);
    }

    [TestMethod]
    public async Task GetFolderByPath_DeeplyNestedFolderExists_ReturnsFolder()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");
        var level1 = rootFolder.AddFolder("level1");
        var level2 = level1.AddFolder("level2");
        var level3 = level2.AddFolder("level3");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root/level1/level2/level3");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("level3", result.Folder.Name);
    }

    [TestMethod]
    public async Task GetFolderByPath_IntermediateFolderMissing_ReturnsFolderNotFound()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");
        var subFolder = rootFolder.AddFolder("subfolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root/nonexistent/deeper");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Found);
        Assert.IsNull(result.Folder);
    }

    [TestMethod]
    public async Task GetFolderByPath_FinalFolderMissing_ReturnsFolderNotFound()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");
        var subFolder = rootFolder.AddFolder("subfolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root/subfolder/nonexistent");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Found);
        Assert.IsNull(result.Folder);
    }

    [TestMethod]
    public async Task GetFolderByPath_QueryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            await _handler!.Handle(null!, CancellationToken.None);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task GetFolderByPath_PathIsNull_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetFolderByPathQuery(null!);

        // Act & Assert
        try
        {
            await _handler!.Handle(query, CancellationToken.None);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task GetFolderByPath_PathIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetFolderByPathQuery(string.Empty);

        // Act & Assert
        try
        {
            await _handler!.Handle(query, CancellationToken.None);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void GetFolderByPathHandler_RepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            _ = new GetFolderByPathHandler(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task GetFolderByPath_MultipleRootsExist_FindsCorrectRoot()
    {
        // Arrange
        var root1 = Folder.CreateRoot("root1");
        var root2 = Folder.CreateRoot("root2");
        var sub2 = root2.AddFolder("subfolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root2")
            .Returns(root2);

        var query = new GetFolderByPathQuery("/root2/subfolder");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("subfolder", result.Folder.Name);
    }

    [TestMethod]
    public async Task GetFolderByPath_CaseInsensitiveFolderName_ReturnsFolderIfExists()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("MyRoot");
        var subFolder = rootFolder.AddFolder("SubFolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("MyRoot")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/MyRoot/SubFolder");

        // Act
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("SubFolder", result.Folder.Name);
    }

    [TestMethod]
    public async Task GetFolderByPath_ImplicitStringToPathConversion_ReturnsFolder()
    {
        // Arrange
        var rootFolder = Folder.CreateRoot("root");
        var subFolder = rootFolder.AddFolder("subfolder");

        _folderRepository!.GetSubtreeWithHierarchyAsync("root")
            .Returns(rootFolder);

        var query = new GetFolderByPathQuery("/root/subfolder");

        // Act - implicitly converts string to FolderPath internally if needed
        var result = await _handler!.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Found);
        Assert.IsNotNull(result.Folder);
        Assert.AreEqual("subfolder", result.Folder.Name);
    }

    [TestMethod]
    public void FolderPath_ImplicitConversion_StringToFolderPath_Works()
    {
        // This test demonstrates the implicit conversion capability
        // Arrange
        string pathString = "/root/folder1/folder2";

        // Act - implicit conversion from string to FolderPath
        Folders.Core.Values.FolderPath path = pathString;

        // Assert
        Assert.IsNotNull(path);
        Assert.AreEqual("/root/folder1/folder2", (string)path);
    }
}
