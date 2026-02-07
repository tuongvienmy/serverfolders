using DomainFundamentals;
using Folders.Core.Aggregates;

namespace Folders.Application.Abstractions;
public interface IFolderRepository : IRepository<Folder>
{
    Task<Folder?> GetByIdAsync(Guid id);
    Task<IEnumerable<Folder>> FindByNameAsync(string? nameFilter = null, bool rootOnly = false);
    Task AddAsync(Folder folder);
    Task UpdateAsync(Folder folder);
    Task DeleteAsync(Folder folder);
}

