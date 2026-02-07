using Folders.Application.Abstractions;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.GetFolders;

public class GetFoldersByNameQueryHandler : IRequestHandler<GetFoldersByNameQuery, IEnumerable<Folder>>
{
    private readonly IFolderRepository _folderRepository;
    public GetFoldersByNameQueryHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
    }
    public async Task<IEnumerable<Folder>> Handle(GetFoldersByNameQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return await _folderRepository.FindByNameAsync(request.SearchTerm, request.rootOnly);
    }
}
