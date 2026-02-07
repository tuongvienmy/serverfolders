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
            .Include(f => f.Items)   // eager load children if you want
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Folder>> FindByNameAsync(string? nameFilter = null, bool rootOnly = false)
    {
        var query = _dbContext.FolderItems
            .OfType<Folder>()
            .Include(f => f.Items)
            .AsQueryable();

        if (rootOnly)
        { 
            query = query.Where(f => f.ParentFolderId == null);
        }
        
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(f => EF.Functions.Like(f.Name, $"%{nameFilter}%"));
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(Folder folder)
    {
        if (folder == null) throw new ArgumentNullException(nameof(folder));
        await _dbContext.FolderItems.AddAsync(folder);
    }

    public Task UpdateAsync(Folder folder)
    {
        if (folder == null) throw new ArgumentNullException(nameof(folder));
        _dbContext.FolderItems.Update(folder);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Folder folder)
    {
        if (folder == null) throw new ArgumentNullException(nameof(folder));
        _dbContext.FolderItems.Remove(folder);
        return Task.CompletedTask;
    }
}