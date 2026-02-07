using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.GetFolderById;

public record GetFolderByIdQuery(Guid Id) : IRequest<Folder?>;

