# SearchAsync() Method - Test Categorization & Implementation Guide

## Overview
This document provides a comprehensive categorization of tests for the `FolderRepository.SearchAsync()` method. The method searches for folders in a hierarchical structure using a Common Table Expression (CTE) with recursive queries in SQL.

## FolderQuery Parameters

The `SearchAsync()` method accepts a `FolderQuery` object with the following parameters:

```csharp
public class FolderQuery
{
    public string? Name { get; set; }                    // Folder name to search for
    public bool RootsOnly { get; set; }                   // Only search root folders (no parent)
    public bool ExactMatch { get; set; }                  // Exact name match vs LIKE search
    public bool IncludeSubFolders { get; set; } = true;  // Include children in results
}
```

---

## Test Categories

### **Category 1: Basic Functionality**

Tests that verify core search behavior and return types.

#### 1.1 Null/Empty Queries
- **Test**: `SearchAsync_WithNullNameFilter_ReturnsEmptyCollection`
- **Purpose**: Verify that searching with null Name returns no results
- **Setup**: Create query with `Name = null`, `RootsOnly = true`
- **Expected**: Returns empty collection
- **Why Important**: Validates boundary condition handling

#### 1.2 Exact Name Matching
- **Test**: `SearchAsync_WithExactNameMatch_ReturnsFolderWithMatchingName`
- **Purpose**: Verify exact folder name matching works
- **Setup**: Create folder, search with `ExactMatch = true`
- **Expected**: Returns folder with exact matching name
- **Why Important**: Core functionality for precise folder lookup

#### 1.3 Return Type Validation
- **Test**: `SearchAsync_ReturnsOnlyFolderObjects`
- **Purpose**: Verify only Folder objects are returned (not File objects)
- **Setup**: Search for folder
- **Expected**: All results are `Folder` type
- **Why Important**: Ensures proper filtering of domain types

---

### **Category 2: Filtering Options**

Tests that verify each filter parameter works correctly and can be combined.

#### 2.1 RootsOnly Filter

**Test: `SearchAsync_WithRootsOnlyTrue_ReturnsOnlyRootFolders`**
- **Purpose**: Verify `RootsOnly = true` returns only folders with no parent
- **SQL Clause**: `AND (0 = 0 OR ParentFolderId IS NULL)`
- **Expected**: All results have `ParentFolderId == null`
- **Use Case**: Finding top-level folders only

**Test: `SearchAsync_WithRootsOnlyFalse_ReturnsAllMatchingFolders`**
- **Purpose**: Verify `RootsOnly = false` returns all matching folders at any level
- **Expected**: Includes both root and nested folders with same name
- **Use Case**: Deep folder searches

#### 2.2 ExactMatch Filter

**Test: `SearchAsync_WithExactMatchTrue_MatchesOnlyExactNames`**
- **Purpose**: Verify exact string matching (`Name = {value}`)
- **SQL Clause**: `OR (1 = 1 AND Name = {query.Name})`
- **Expected**: Only folders with exact name match
- **Use Case**: Precise folder identification

**Test: `SearchAsync_WithExactMatchFalse_WorksWithGivenName`**
- **Purpose**: Verify partial matching using LIKE operator
- **SQL Clause**: `OR (0 = 0 AND Name LIKE '%{query.Name}%')`
- **Expected**: Folders containing the search text
- **Use Case**: Pattern-based folder discovery

#### 2.3 IncludeSubFolders Filter

**Test: `SearchAsync_WithIncludeSubFoldersTrue_ReturnsFromEntireHierarchy`**
- **Purpose**: Verify recursion includes all descendants
- **SQL Clause**: Recursive CTE continues when `IncludeSubFolders = 1`
- **Expected**: Finds folder at any depth
- **Use Case**: Deep folder searches

**Test: `SearchAsync_WithIncludeSubFoldersFalse_FindsOnlyAnchorFolders`**
- **Purpose**: Verify recursion stops when `IncludeSubFolders = 0`
- **SQL Clause**: `WHERE {query.IncludeSubFolders} = 1` in recursion
- **Expected**: Only anchor (first-level matching) folders
- **Use Case**: Top-level only searches

#### 2.4 Combined Filters

**Test: `SearchAsync_WithCombinedFilters_AppliesBothConstraints`**
- **Purpose**: Verify multiple filters work together
- **Setup**: `RootsOnly = true` AND `ExactMatch = true`
- **Expected**: Only root folders with exact name match
- **Use Case**: Precise root-level folder lookup

---

### **Category 3: Edge Cases**

Tests for boundary conditions and special scenarios.

#### 3.1 No Results

**Test**: `SearchAsync_WithNoMatches_ReturnsEmptyCollection`
- **Purpose**: Handle searches with no matching folders
- **Expected**: Empty collection, not null or exception
- **Why Important**: Graceful handling of missing data

#### 3.2 Single vs Multiple Results

**Test**: `SearchAsync_WithSingleResult_ReturnsSingleFolderCorrectly`
- **Purpose**: Verify single result is returned correctly
- **Expected**: Collection with one element

**Test**: `SearchAsync_WithDuplicateNames_ReturnsAllMatches`
- **Purpose**: Multiple folders can have the same name
- **Expected**: All matching folders returned
- **Why Important**: Handles duplicate names in different branches

#### 3.3 Case Sensitivity

**Test**: `SearchAsync_CaseSensitivity_FollowsDatabaseCollation`
- **Purpose**: Verify case sensitivity follows SQL collation rules
- **Note**: SQLite is case-insensitive by default
- **Expected**: Behavior depends on database configuration

#### 3.4 Special Characters

**Test**: `SearchAsync_WithSpecialCharacters_FindsFolderCorrectly`
- **Purpose**: Folders with special characters (-, _, .) are searchable
- **Expected**: Exact match finds folder with special chars
- **Why Important**: Real-world folder names often contain special characters

---

### **Category 4: Hierarchy Scenarios**

Tests for folder relationship and tree structure behaviors.

#### 4.1 Nested Structures

**Test**: `SearchAsync_InNestedStructure_FindsCorrectChild`
- **Purpose**: Find folder in simple parent-child hierarchy
- **Setup**: 3-level structure (Root → Documents → Important)
- **Expected**: Finds folder at any level when `IncludeSubFolders = true`

#### 4.2 Deep Nesting

**Test**: `SearchAsync_InDeeplyNestedStructure_ReturnsTargetFolder`
- **Purpose**: CTE recursion works for deep structures (5+ levels)
- **Setup**: Level0 → Level1 → Level2 → Level3 → TargetFolder
- **Expected**: Finds deeply nested folder
- **Why Important**: Validates recursive CTE performance

#### 4.3 Orphaned Folders

**Test**: `SearchAsync_WithOrphanedFolder_FindsItCorrectly`
- **Purpose**: Root folders (no parent) are searchable
- **Expected**: Finds folder with `ParentFolderId == null`
- **Why Important**: Validates handling of root-level folders

#### 4.4 Subtree Inclusion

**Test**: `SearchAsync_IncludesChildren_WhenIncludeSubFoldersTrue`
- **Purpose**: Verify children are included in results when recursing
- **Expected**: Results include ancestor and descendants
- **Why Important**: Validates CTE recursive clause behavior

---

### **Category 5: Performance/Stress**

Tests for handling large datasets and complex structures.

#### 5.1 Large Number of Folders

**Test**: `SearchAsync_WithManyFolders_ReturnsCorrectResults`
- **Purpose**: Search performance with 100+ folders
- **Setup**: Create 100 root folders
- **Expected**: Correct folder found in reasonable time
- **Why Important**: Identifies performance bottlenecks

#### 5.2 Complex Hierarchies

**Test**: `SearchAsync_InComplexHierarchy_ReturnsCorrectResults`
- **Purpose**: Complex structure with deep nesting and siblings
- **Setup**: Deep chain (10 levels) + sibling branches
- **Expected**: Correct folder found despite complexity
- **Why Important**: Real-world folder structures often have this pattern

---

## Implementation Patterns

### Basic Test Structure

```csharp
[TestMethod]
public async Task SearchAsync_[Condition]_[Expected]()
{
    // ARRANGE: Set up folders
    var folder = Folder.CreateRoot("FolderName");
    await _repository.AddAsync(folder);
    await _dbContext.SaveChangesAsync();

    // Create query with specific parameters
    var query = new FolderQuery
    {
        Name = "FolderName",
        RootsOnly = false,
        ExactMatch = true,
        IncludeSubFolders = false
    };

    // ACT: Execute search
    var result = await _repository.SearchAsync(query);

    // ASSERT: Verify results
    Assert.AreEqual(1, result.Count);
    Assert.AreEqual("FolderName", result[0].Name);
}
```

### Creating Nested Structures

```csharp
// Simple hierarchy
var root = Folder.CreateRoot("Root");
var child = root.AddFolder("Child");
var grandchild = child.AddFolder("Grandchild");

// Save only the root; children are saved with it
await _repository.AddAsync(root);
await _dbContext.SaveChangesAsync();
```

### Testing with Filters

```csharp
// Test all combinations:
// 1. RootsOnly variations
// 2. ExactMatch variations  
// 3. IncludeSubFolders variations
// 4. Combinations of all three

// For each combination, verify the SQL WHERE clause conditions are met
```

---

## SQL Query Structure

The `SearchAsync` method uses this CTE pattern:

```sql
WITH FolderTree AS (
    -- ANCHOR: Find starting folders matching criteria
    SELECT *
    FROM FolderItems
    WHERE FolderItemType = 'Folder'
      AND (Name IS NULL OR exact/like match)
      AND (RootsOnly constraint)

    UNION ALL

    -- RECURSION: Get descendants of starting folders
    SELECT f.*
    FROM FolderItems f
    INNER JOIN FolderTree t ON f.ParentFolderId = t.Id
    WHERE IncludeSubFolders = 1 AND f.FolderItemType = 'Folder'
)
SELECT DISTINCT * FROM FolderTree
```

### Key Points:
1. **Anchor**: Finds initial matching folders
2. **Recursion**: Includes children if `IncludeSubFolders = true`
3. **DISTINCT**: Removes duplicates
4. **Filtering**: Applied at each level

---

## Common Testing Mistakes to Avoid

### ❌ Don't
```csharp
// Assuming LIKE wildcards are auto-added
var query = new FolderQuery { Name = "Test", ExactMatch = false };
// This will search for exact "Test", not "%Test%"

// Assuming Name filter works with null
var query = new FolderQuery { Name = null }; // Returns empty
```

### ✅ Do
```csharp
// Explicitly include LIKE patterns if needed
var query = new FolderQuery { Name = "%Test%", ExactMatch = false };

// Use empty checks for queries that should return something
var query = new FolderQuery { Name = "ExactName", ExactMatch = true };
```

---

## Test Summary

| Category | Test Count | Focus |
|----------|-----------|-------|
| Basic Functionality | 3 | Core search behavior |
| Filtering Options | 8 | Individual & combined filters |
| Edge Cases | 6 | Boundaries & special conditions |
| Hierarchy | 5 | Folder relationships |
| Performance | 2 | Scalability & complexity |
| **Total** | **24+** | Comprehensive coverage |

---

## Adding New Tests

When adding new `SearchAsync` tests:

1. **Choose a category** based on what you're testing
2. **Use descriptive names**: `SearchAsync_[Condition]_[Expected]`
3. **Follow the AAA pattern**: Arrange, Act, Assert
4. **Test one thing**: Each test should verify one behavior
5. **Use helper methods** for creating hierarchies
6. **Document assumptions** about SQL behavior

---

## References

- **FolderQuery**: `Src\Folders.Application\Abstractions\FolderQuery.cs`
- **FolderRepository**: `Src\Folders.Infrastructure\Persistence\Repositories\FolderRepository.cs`
- **Tests**: `Tests\Folders.Infrastructure.Tests\Repositories\FolderRepositorySearchAsyncTests.cs`

