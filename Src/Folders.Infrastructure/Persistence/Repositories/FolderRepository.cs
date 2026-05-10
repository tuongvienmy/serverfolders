using DomainFundamentals;
using Folders.Core.Aggregates;
using Folders.Application.Abstractions;
using Folders.Infrastructure.Persistence.DatabaseContexts;
using Microsoft.EntityFrameworkCore;

namespace Folders.Infrastructure.Persistence;
public class FolderRepository : IFolderRepository
{
    private readonly FoldersDbContext _dbContext;

    public IUnitOfWork UnitOfWork => _dbContext;

    public FolderRepository(FoldersDbContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<Folder?> GetByIdAsync(Guid id)
    {
        return await _dbContext.FolderItems.OfType<Folder>()
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    public async Task<IReadOnlyList<Folder>> GetRootsAsync()
    {
        return await _dbContext.FolderItems
            .OfType<Folder>()
            .Where(f => f.ParentFolderId == null)
            .ToListAsync();
    }
    public async Task<IReadOnlyList<Folder>> GetChildrenAsync(Guid parentId)
    {
        return await _dbContext.FolderItems
            .OfType<Folder>()
            .Where(f => f.ParentFolderId == parentId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Folder>> GetSubtreeAsync(Guid rootId)
    {
        var sql = @"
        WITH FolderTree AS (
            SELECT *
            FROM FolderItems
            WHERE Id = {0} AND FolderItemType = 'Folder'

            UNION ALL

            SELECT f.*
            FROM FolderItems f
            INNER JOIN FolderTree t ON f.ParentFolderId = t.Id AND f.FolderItemType = 'Folder'
        )
        SELECT *
        FROM FolderTree";

        var items = await _dbContext.FolderItems
                            .FromSqlRaw(sql, rootId)
                            .AsNoTracking()
                            .ToListAsync();

        return items.OfType<Folder>().ToList();
    }

    /// <summary>
    /// Gets the subtree and builds the complete folder hierarchy with items loaded into their parent folders.
    /// </summary>
    public async Task<Folder?> GetSubtreeWithHierarchyAsync(Guid rootId)
    {
        var subtree = (await GetSubtreeAsync(rootId)).ToList();
        if (subtree.Count == 0)
            return null;

        // Build the tree hierarchy
        BuildTree(subtree);

        return subtree.FirstOrDefault(f => f.Id == rootId); ;
    }

    public async Task<Folder?> GetSubtreeWithHierarchyAsync(string rootName)
    {
        var subtree = await SearchAsync(new FolderQuery { Name = rootName, ExactMatch = true , RootsOnly = true, IncludeSubFolders = true });
        if (subtree.Count == 0)
            return null;

        // Build the tree hierarchy
        BuildTree(subtree);

        return subtree.FirstOrDefault(f => f.ParentFolderId == null && f.Name == rootName);
    }

    public async Task<IReadOnlyList<Folder>> SearchAsync(FolderQuery query)
    {
        FormattableString sql = $@"WITH FolderTree AS (
                                    -- ANCHOR: Find the starting folders
                                    SELECT *
                                    FROM FolderItems
                                    WHERE FolderItemType = 'Folder'
                                      AND ({query.Name} IS NULL
                                           OR ({query.ExactMatch} = 1 AND Name = {query.Name})
                                           OR ({query.ExactMatch} = 0 AND Name LIKE '%{query.Name}%'))
                                      -- If RootsOnly is true, only pick folders that have no parent
                                      AND ({query.RootsOnly} = 0 OR ParentFolderId IS NULL)

                                    UNION ALL

                                    -- RECURSION: Get everything beneath those starting folders
                                    SELECT f.*
                                    FROM FolderItems f
                                    INNER JOIN FolderTree t ON f.ParentFolderId = t.Id
                                    WHERE {query.IncludeSubFolders} = 1 AND f.FolderItemType = 'Folder'
                                )

                                -- FINAL SELECT: Use DISTINCT to avoid duplicates if a name appears twice in one branch
                                SELECT DISTINCT *
                                FROM FolderTree ";
        
        var items = await _dbContext.FolderItems
                .FromSqlInterpolated(sql)
                .AsNoTracking()
                .ToListAsync();

        return items.OfType<Folder>().ToList();

    }

    /// <summary>
    /// Organizes a flat list of folders into a proper folder hierarchy by loading child items into their parent folders.
    /// </summary>
    private static void BuildTree(IEnumerable<Folder> folders)
    {
        var lookup = folders.ToLookup(f => f.ParentFolderId);

        foreach (var folder in folders)
        {
            var children = lookup[folder.Id].ToList();
            if (children.Count > 0)
            {
                folder.LoadItems(children.Cast<FolderItem>());
            }
        }
    }

    public async Task<IReadOnlyList<Folder>> FindByNameAsync(string? nameFilter = null, bool rootOnly = true, bool exactMatch = true)
    {
        var query = new FolderQuery();
        if (nameFilter != null)
        {
            query.Name = nameFilter;
            query.RootsOnly = rootOnly;
            query.ExactMatch = exactMatch;
            query.IncludeSubFolders = true;
        }
        else if(rootOnly == true)
        {
            query.IncludeSubFolders = false;
        }

        return await SearchAsync(query);
    }

    public async Task AddAsync(Folder folder)
    {
        if (folder is null) throw new ArgumentNullException(nameof(folder));
        await _dbContext.FolderItems.AddAsync(folder);
    }

    public Task UpdateAsync(Folder folder)
    {
        if (folder is null) throw new ArgumentNullException(nameof(folder));
        _dbContext.FolderItems.Update(folder);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Folder folder)
    {
        if (folder is null) throw new ArgumentNullException(nameof(folder));
        _dbContext.FolderItems.Remove(folder);
        return Task.CompletedTask;
    }
}