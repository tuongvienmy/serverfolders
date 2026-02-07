using Folders.Application.Abstractions;
using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.CreateRoot;
public class CreateRootCommandHandler: IRequestHandler<CreateRootCommand, Folder>
{    
    private readonly IFolderRepository _folderRepo;

    public CreateRootCommandHandler(IFolderRepository folderRepo)
    {
        _folderRepo = folderRepo;
    }
    public async Task<Folder> Handle(CreateRootCommand newFolderCommand, CancellationToken cancellationToken)
    {
        var folder = Folder.CreateRoot(newFolderCommand.Name);
        await _folderRepo.AddAsync(folder);
                
        await _folderRepo.UnitOfWork.SaveChangesAsync();
        return folder;
    }
}
