using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.GetFolders;

public record GetFoldersByNameQuery(string? SearchTerm = null, bool rootOnly = true) : IRequest<IEnumerable<Folder>>;
