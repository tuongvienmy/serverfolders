using Folders.Application.DTOs;
using MediatR;

namespace Folders.Application.UseCases.CreateRoot;

public record CreateRootCommand(string Name) : IRequest<FolderDto>;
