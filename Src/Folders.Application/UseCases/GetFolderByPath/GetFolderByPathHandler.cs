using Folders.Application.Abstractions;
using Folders.Application.DTOs;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.GetFolderByPath;

public class GetFolderByPathHandler : IRequestHandler<GetFolderByPathQuery, GetFolderByPathResult>
{
    private readonly IFolderRepository _folderRepository;

    public GetFolderByPathHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
    }

    public async Task<GetFolderByPathResult> Handle(GetFolderByPathQuery request, CancellationToken cancellationToken)
    {
        if (request == null) 
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(request.Path));

        // Parse the path string into segments
        var segments = request.Path
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        if (segments.Count == 0)
            return new GetFolderByPathResult(null, false);

        // Find the root folder by name
        var root = await _folderRepository.GetSubtreeWithHierarchyAsync(segments[0]);        

        if (root is null)
            return new GetFolderByPathResult(null, false);

        // Navigate through the path segments
        var currentFolder = root;
        for (int i = 1; i < segments.Count; i++)
        {
            var segment = segments[i];
            currentFolder = currentFolder.Get(segment) as Folder;

            if (currentFolder is null)
                return new GetFolderByPathResult(null, false);            
        }

        var resultDto = currentFolder.ToDto();

        return new GetFolderByPathResult(resultDto, true);
    }
}
