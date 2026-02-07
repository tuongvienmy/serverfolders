using Folders.Application.Abstractions;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.GetFolderById;

public class GetFolderByIdQueryHandler: IRequestHandler<GetFolderByIdQuery, Folder?>
{
    private readonly IFolderRepository _folderRepository;
    public GetFolderByIdQueryHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
    }
    public async Task<Folder?> Handle(GetFolderByIdQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return await _folderRepository.GetByIdAsync(request.Id);
    }
}
