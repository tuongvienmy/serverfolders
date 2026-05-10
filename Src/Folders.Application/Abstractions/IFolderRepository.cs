using DomainFundamentals;
using Folders.Core.Aggregates;

namespace Folders.Application.Abstractions;
public interface IFolderRepository : IRepository<Folder>
{
    Task<Folder?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Folder>> FindByNameAsync(string? nameFilter = null, bool rootOnly = true, bool exactMatch = true);
    Task<IReadOnlyList<Folder>> GetRootsAsync();
    Task<IReadOnlyList<Folder>> GetChildrenAsync(Guid parentId);
    Task<IReadOnlyList<Folder>> GetSubtreeAsync(Guid rootId);
    Task<Folder?> GetSubtreeWithHierarchyAsync(string rootName);
    Task<Folder?> GetSubtreeWithHierarchyAsync(Guid rootId);

    Task AddAsync(Folder folder);
    Task UpdateAsync(Folder folder);
    Task DeleteAsync(Folder folder);
}

