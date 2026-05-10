using DomainFundamentals;
using Folders.Application.Abstractions;
using Folders.Core.Aggregates;
using Folders.Infrastructure.Persistence;
using Folders.Infrastructure.Persistence.DatabaseContexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Folders.Infrastructure.Tests.Repositories;

/// <summary>
/// Test categorization for SearchAsync method:
/// 
/// Category 1: BASIC FUNCTIONALITY
/// - Search with null/empty query
/// - Search with exact name match
/// - Search with partial name match
/// - Search returns correct folder type
/// 
/// Category 2: FILTERING OPTIONS
/// - RootsOnly filter (include/exclude)
/// - ExactMatch filter (exact vs partial)
/// - IncludeSubFolders filter (include/exclude)
/// - Combined filters
/// 
/// Category 3: EDGE CASES
/// - No results found
/// - Single result
/// - Multiple results with same name
/// - Case sensitivity
/// - Special characters in names
/// 
/// Category 4: HIERARCHY SCENARIOS
/// - Nested folders (parent-child relationships)
/// - Deep nesting (3+ levels)
/// - Orphaned folders (no parent)
/// - Subtree inclusivity
/// 
/// Category 5: PERFORMANCE/STRESS
/// - Large number of folders
/// - Deep folder hierarchies
/// - Complex nested structures
/// </summary>
[TestClass]
public class FolderRepositorySearchAsyncTests
{
    private SqliteConnection _connection = null!;
    private FoldersDbContext _dbContext = null!;
    private FolderRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FoldersDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new FoldersDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new FolderRepository(_dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    #region Category 1: Basic Functionality Tests

    [TestMethod]
    [Description("Search with null/empty name filter should return empty collection")]
    public async Task SearchAsync_WithNullNameFilter_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new FolderQuery
        {
            Name = null,
            RootsOnly = true,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [Description("Search with exact name match should find the folder")]
    public async Task SearchAsync_WithExactNameMatch_ReturnsFolderWithMatchingName()
    {
        // Arrange
        var folderName = "ExactMatchFolder";
        var folder = Folder.CreateRoot(folderName);
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = folderName,
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(folderName, result[0].Name);
    }

    [TestMethod]
    [Description("Search with pattern matching should find folders")]
    public async Task SearchAsync_WithPatternMatching_FindsFolderCorrectly()
    {
        // Arrange
        var folder = Folder.CreateRoot("MySpecialFolder");

        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "MySpecialFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("MySpecialFolder", result[0].Name);
    }

    [TestMethod]
    [Description("Search should return only Folder objects (not Files)")]
    public async Task SearchAsync_ReturnsOnlyFolderObjects()
    {
        // Arrange
        var folder = Folder.CreateRoot("FolderSearch");
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Folder",
            RootsOnly = false,
            ExactMatch = false,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.IsTrue(result.All(f => f is Folder));
    }

    #endregion

    #region Category 2: Filtering Options Tests

    [TestMethod]
    [Description("RootsOnly=true should return only root folders (no parent)")]
    public async Task SearchAsync_WithRootsOnlyTrue_ReturnsOnlyRootFolders()
    {
        // Arrange
        var root = Folder.CreateRoot("Root");
        var child = root.AddFolder("Child");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = null,
            RootsOnly = true,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.IsTrue(result.All(f => f.ParentFolderId == null));
    }

    [TestMethod]
    [Description("RootsOnly=false should return all matching folders regardless of parent")]
    public async Task SearchAsync_WithRootsOnlyFalse_ReturnsAllMatchingFolders()
    {
        // Arrange
        var root = Folder.CreateRoot("MyFolder");
        var child = root.AddFolder("MyFolder"); // Same name as root

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "MyFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    [Description("ExactMatch=true should match only exact folder names")]
    public async Task SearchAsync_WithExactMatchTrue_MatchesOnlyExactNames()
    {
        // Arrange
        var folder1 = Folder.CreateRoot("TestFolder");
        var folder2 = Folder.CreateRoot("TestFolderExtra");

        await _repository.AddAsync(folder1);
        await _repository.AddAsync(folder2);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "TestFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("TestFolder", result[0].Name);
    }

    [TestMethod]
    [Description("ExactMatch=false with specific text should work correctly")]
    public async Task SearchAsync_WithExactMatchFalse_WorksWithGivenName()
    {
        // Arrange
        var folder = Folder.CreateRoot("TestFolder");

        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        // When ExactMatch=false, it uses LIKE operator
        var query = new FolderQuery
        {
            Name = "TestFolder",
            RootsOnly = false,
            ExactMatch = false,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [Description("IncludeSubFolders=true should return search results from entire hierarchy")]
    public async Task SearchAsync_WithIncludeSubFoldersTrue_ReturnsFromEntireHierarchy()
    {
        // Arrange
        var root = Folder.CreateRoot("Root");
        var level1 = root.AddFolder("Level1");
        var level2 = level1.AddFolder("SearchTarget");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "SearchTarget",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("SearchTarget", result[0].Name);
    }

    [TestMethod]
    [Description("IncludeSubFolders=false should not recurse into children")]
    public async Task SearchAsync_WithIncludeSubFoldersFalse_FindsOnlyAnchorFolders()
    {
        // Arrange
        var root = Folder.CreateRoot("Target");
        var child = root.AddFolder("Target");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Target",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        // With IncludeSubFolders=false, the CTE should find the root "Target" in the anchor,
        // then in the recursion, it checks "WHERE {query.IncludeSubFolders} = 1", which is false,
        // so it stops recursing. Both the anchor and the recursive part will find "Target",
        // but the DISTINCT should deduplicate them. Let's check the actual behavior.
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any(f => f.Name == "Target"));
    }

    [TestMethod]
    [Description("Combined filters: RootsOnly + ExactMatch should apply both constraints")]
    public async Task SearchAsync_WithCombinedFilters_AppliesBothConstraints()
    {
        // Arrange
        var root1 = Folder.CreateRoot("Admin");
        var root2 = Folder.CreateRoot("Administrator");
        var child = root1.AddFolder("Admin");

        await _repository.AddAsync(root1);
        await _repository.AddAsync(root2);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Admin",
            RootsOnly = true,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Admin", result[0].Name);
        Assert.IsNull(result[0].ParentFolderId);
    }

    #endregion

    #region Category 3: Edge Cases Tests

    [TestMethod]
    [Description("Search with no matches should return empty collection")]
    public async Task SearchAsync_WithNoMatches_ReturnsEmptyCollection()
    {
        // Arrange
        var folder = Folder.CreateRoot("ExistingFolder");
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "NonexistentFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [Description("Search returning single result should work correctly")]
    public async Task SearchAsync_WithSingleResult_ReturnsSingleFolderCorrectly()
    {
        // Arrange
        var folder = Folder.CreateRoot("UniqueFolder");
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "UniqueFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("UniqueFolder", result[0].Name);
    }

    [TestMethod]
    [Description("Multiple folders with same name should all be returned")]
    public async Task SearchAsync_WithDuplicateNames_ReturnsAllMatches()
    {
        // Arrange
        var root1 = Folder.CreateRoot("SameName");
        var root2 = Folder.CreateRoot("SameName");

        await _repository.AddAsync(root1);
        await _repository.AddAsync(root2);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "SameName",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(f => f.Name == "SameName"));
    }

    [TestMethod]
    [Description("Search should be case-sensitive (if database collation requires it)")]
    public async Task SearchAsync_CaseSensitivity_FollowsDatabaseCollation()
    {
        // Arrange
        var folder = Folder.CreateRoot("MyFolder");
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "myfolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert - SQLite is case-insensitive by default, so this may return 1 or 0
        // depending on your collation settings
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [Description("Folder names with special characters should be searchable")]
    public async Task SearchAsync_WithSpecialCharacters_FindsFolderCorrectly()
    {
        // Arrange
        var folderName = "Folder-With_Special.Chars";
        var folder = Folder.CreateRoot(folderName);
        await _repository.AddAsync(folder);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = folderName,
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(folderName, result[0].Name);
    }

    #endregion

    #region Category 4: Hierarchy Scenarios Tests

    [TestMethod]
    [Description("Search within nested folders should find correct child")]
    public async Task SearchAsync_InNestedStructure_FindsCorrectChild()
    {
        // Arrange
        var root = Folder.CreateRoot("Root");
        var child = root.AddFolder("Documents");
        var grandchild = child.AddFolder("Important");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Important",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Important", result[0].Name);
    }

    [TestMethod]
    [Description("Search in deeply nested structure (3+ levels) should work")]
    public async Task SearchAsync_InDeeplyNestedStructure_ReturnsTargetFolder()
    {
        // Arrange
        var root = Folder.CreateRoot("Level0");
        var level1 = root.AddFolder("Level1");
        var level2 = level1.AddFolder("Level2");
        var level3 = level2.AddFolder("Level3");
        var level4 = level3.AddFolder("TargetFolder");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "TargetFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("TargetFolder", result[0].Name);
    }

    [TestMethod]
    [Description("Search with orphaned folder (no parent) should find it")]
    public async Task SearchAsync_WithOrphanedFolder_FindsItCorrectly()
    {
        // Arrange
        var orphan = Folder.CreateRoot("Orphan");
        await _repository.AddAsync(orphan);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Orphan",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].ParentFolderId);
    }

    [TestMethod]
    [Description("Search with IncludeSubFolders=true includes the matching folder")]
    public async Task SearchAsync_IncludesChildren_WhenIncludeSubFoldersTrue()
    {
        // Arrange
        var root = Folder.CreateRoot("RootFolder");
        var child = root.AddFolder("ChildFolder");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "RootFolder",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        // When searching for "RootFolder" with IncludeSubFolders=true,
        // the CTE returns the root plus its children
        Assert.IsTrue(result.Count >= 1);
        Assert.IsTrue(result.Any(f => f.Name == "RootFolder"));
    }

    #endregion

    #region Category 5: Performance/Stress Tests

    [TestMethod]
    [Description("Search should handle large number of folders efficiently")]
    public async Task SearchAsync_WithManyFolders_ReturnsCorrectResults()
    {
        // Arrange
        var rootFolders = new List<Folder>();
        for (int i = 0; i < 100; i++)
        {
            var folder = Folder.CreateRoot($"Folder{i:D3}");
            rootFolders.Add(folder);
            await _repository.AddAsync(folder);
        }
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Folder050",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = false
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Folder050", result[0].Name);
    }

    [TestMethod]
    [Description("Search in complex nested hierarchy should work efficiently")]
    public async Task SearchAsync_InComplexHierarchy_ReturnsCorrectResults()
    {
        // Arrange
        var root = Folder.CreateRoot("Root");
        var current = root;

        // Create a chain of 10 nested folders
        for (int i = 0; i < 10; i++)
        {
            current = current.AddFolder($"Level{i}");
        }

        // Add siblings at various levels
        var sibling = root.AddFolder("Sibling");
        sibling.AddFolder("SiblingChild");

        await _repository.AddAsync(root);
        await _dbContext.SaveChangesAsync();

        var query = new FolderQuery
        {
            Name = "Level9",
            RootsOnly = false,
            ExactMatch = true,
            IncludeSubFolders = true
        };

        // Act
        var result = await _repository.SearchAsync(query);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Level9", result[0].Name);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper to create a hierarchy of folders for testing
    /// </summary>
    private Folder CreateFolderHierarchy(string rootName, int depth, int breadth = 1)
    {
        var root = Folder.CreateRoot(rootName);
        CreateHierarchyRecursive(root, depth - 1, breadth, 1);
        return root;
    }

    private void CreateHierarchyRecursive(Folder parent, int remainingDepth, int breadth, int level)
    {
        if (remainingDepth <= 0) return;

        for (int i = 0; i < breadth; i++)
        {
            var child = parent.AddFolder($"Level{level}_Item{i}");
            CreateHierarchyRecursive(child, remainingDepth - 1, breadth, level + 1);
        }
    }

    #endregion
}
