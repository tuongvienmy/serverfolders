using Folders.Core.Aggregates;
using MediatR;

namespace Folders.Application.UseCases.CreateRoot;

public record CreateRootCommand(string Name) : IRequest<Folder>;
