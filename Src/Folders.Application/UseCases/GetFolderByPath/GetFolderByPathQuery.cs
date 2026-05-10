using MediatR;

namespace Folders.Application.UseCases.GetFolderByPath;

public record GetFolderByPathQuery(string Path) : IRequest<GetFolderByPathResult>;
