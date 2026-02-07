using Folders.Core.Aggregates;
using Folders.Core.Values;

namespace Folders.Core.Tests;
[TestClass]
public sealed class FolderTests
{
    [TestMethod]
    public void Folder_NewFolder_CreateFolderWithNoParent()
    {
        var folder = Folder.CreateRoot("TestFolder");
        Assert.IsNotNull(folder);
        Assert.AreEqual("TestFolder", folder.Name);
        Assert.IsNull(folder.ParentFolder);
    }

    [TestMethod]
    public void Folder_AddFolder_SubFolderHasParent()
    {
        var parentFolder = Folder.CreateRoot("ParentFolder");
        var subFolder = parentFolder.AddFolder("SubFolder");
        
        Assert.IsNotNull(subFolder);
        Assert.AreEqual("SubFolder", subFolder.Name);
        Assert.AreEqual(parentFolder, subFolder.ParentFolder);
    }

    [TestMethod]
    public void Folder_AddFolder_WithExistingName_AppendedWithCounter()
    {
        var parentFolder = Folder.CreateRoot("ParentFolder");
        parentFolder.AddFolder("SubFolder");
        var duplicated = parentFolder.AddFolder("SubFolder");
        Assert.IsNotNull(duplicated);
        Assert.AreEqual("SubFolder (1)", duplicated.Name);
    }
    [TestMethod]
    public void Folder_Get_ItemExists_ReturnsItem()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder");
        
        var item = folder.Get("SubFolder");
        
        Assert.IsNotNull(item);
        Assert.AreEqual("SubFolder", item.Name);
    }
    [TestMethod]
    public void Folder_Get_ItemDoesNotExist_ReturnsNull()
    {
        var folder = Folder.CreateRoot("TestFolder");
        
        var item = folder.Get("NonExistentItem");
        
        Assert.IsNull(item);
    }
    [TestMethod]
    public void Folder_FindAll_ItemsExist_ReturnsMatchingItems()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("SubFolder", partialMatch: true).ToList();
        
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder1"));
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder2"));
    }
    [TestMethod]
    public void Folder_FindAll_NoItems_ReturnsEmptyCollection()
    {
        var folder = Folder.CreateRoot("TestFolder");
        
        var results = folder.FindAll("NonExistentItem").ToList();
        
        Assert.AreEqual(0, results.Count);
    }
    [TestMethod]
    public void Folder_FindAll_PartialMatch_ReturnsMatchingItems()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("Sub", partialMatch: true).ToList();
        
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder1"));
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder2"));
    }
    [TestMethod]
    public void Folder_FindAll_PartialMatch_TypeFilter_ReturnsItemsOfType()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("SubFolder", partialMatch:true, typeFilter: typeof(Folder)).ToList();
        
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.All(i => i is Folder));
    }
    [TestMethod]
    public void Folder_FindAll_FullMatch_TypeFilter_ReturnsItemsOfType()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("SubFolder1", typeFilter: typeof(Folder)).ToList();
        
        Assert.AreEqual(1, results.Count);
        Assert.IsTrue(results.All(i => i is Folder));
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder1"));
    }

    [TestMethod]
    public void Folder_FindAll_PartialMatch_TypeFilter_ReturnsEmptyForNonMatchingType()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("SubFolder", partialMatch: true, typeFilter: typeof(Aggregates.File)).ToList();
        
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Folder_FindAll_FullMatch_ReturnsMatchingItems()
    {
        var folder = Folder.CreateRoot("TestFolder");
        folder.AddFolder("SubFolder1");
        folder.AddFolder("SubFolder2");
        
        var results = folder.FindAll("SubFolder1").ToList();
        
        Assert.AreEqual(1, results.Count);
        Assert.IsTrue(results.Any(i => i.Name == "SubFolder1"));
    }

    [TestMethod]
    public void Folder_AddFile_FileAddedToFolder()
    {
        var folder = Folder.CreateRoot("TestFolder");
        var file = folder.AddFile("TestFile.txt");                
        
        Assert.IsNotNull(folder.Get(file.Name) is not null);
        Assert.IsTrue(folder.Get(file.Name) is Core.Aggregates.File);
        Assert.IsTrue(folder.Files.Contains(file));
        Assert.AreEqual(1, folder.NumberOfItems);
    }

    [TestMethod]
    public void Folder_AddFolder_AddFile_FindAll_DifferentParameters_ReturnsAccordingly()
    {
        var folder = Folder.CreateRoot("TestFolder");
        var subFolder = folder.AddFolder("TestSubFolder");
        var file = folder.AddFile("TestFile.txt");
                     
        var results = folder.FindAll("Test", true, typeof(Aggregates.File)).ToList();
        
        Assert.AreEqual(1, results.Count);
        Assert.IsTrue(results.Any(i => i.Name == "TestFile.txt"));
        Assert.IsTrue(results.Any(i => i is Core.Aggregates.File));

        results = folder.FindAll("Test", true).ToList();
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Any(i => i.Name == "TestSubFolder"));
        Assert.IsTrue(results.Any(i => i.Name == "TestFile.txt"));
    }
}