# SearchAsync() Method - Quick Reference

## Test Categorization Summary

### 5 Main Categories:

#### 1. **Basic Functionality** (3 tests)
- Null/empty query handling
- Exact name matching
- Type validation (Folder vs File)

#### 2. **Filtering Options** (8 tests)
- `RootsOnly` filter (root folders only)
- `ExactMatch` filter (exact vs partial)
- `IncludeSubFolders` filter (hierarchy depth)
- Combined filter interactions

#### 3. **Edge Cases** (6 tests)
- No results found
- Single vs multiple results
- Duplicate names
- Case sensitivity
- Special characters

#### 4. **Hierarchy Scenarios** (5 tests)
- Nested folder structures
- Deep nesting (5+ levels)
- Orphaned folders
- Subtree inclusion with recursion

#### 5. **Performance/Stress** (2 tests)
- Large datasets (100+ folders)
- Complex hierarchies (deep + siblings)

---

## Test Implementation Template

```csharp
[TestMethod]
[Description("What this test verifies")]
public async Task SearchAsync_[Scenario]_[Expected]()
{
    // ARRANGE
    var rootFolder = Folder.CreateRoot("RootName");
    var subFolder = rootFolder.AddFolder("SubName");
    await _repository.AddAsync(rootFolder);
    await _dbContext.SaveChangesAsync();

    var query = new FolderQuery
    {
        Name = "SearchTerm",
        RootsOnly = false,      // Search all levels or roots only
        ExactMatch = true,      // Exact match or LIKE search
        IncludeSubFolders = true // Include descendants or not
    };

    // ACT
    var result = await _repository.SearchAsync(query);

    // ASSERT
    Assert.AreEqual(expectedCount, result.Count);
    Assert.IsTrue(result.All(f => f.Name == "ExpectedName"));
}
```

---

## Key Filter Behaviors

### RootsOnly
- **true**: Only folders with `ParentFolderId == null`
- **false**: All matching folders regardless of parent

### ExactMatch
- **true**: `Name = {exactValue}`
- **false**: `Name LIKE '%{value}%'`

### IncludeSubFolders
- **true**: CTE recursion continues; includes all descendants
- **false**: CTE stops after anchor; only direct matches

---

## Running the Tests

```bash
# Run all SearchAsync tests
dotnet test --filter "FullyQualifiedName~SearchAsync"

# Run specific category
dotnet test --filter "FullyQualifiedName~SearchAsync AND FullyQualifiedName~Nested"

# Run with details
dotnet test --logger "console;verbosity=detailed"
```

---

## Test File Location

`Tests\Folders.Infrastructure.Tests\Repositories\FolderRepositorySearchAsyncTests.cs`

## Documentation

`Tests\Folders.Infrastructure.Tests\Repositories\SEARCH_ASYNC_TEST_GUIDE.md`

---

## Total Test Coverage

- **24+ Tests** across 5 categories
- **All Passing** ✅
- Coverage includes: basic ops, all filters, edge cases, hierarchies, performance

