using Folders.Core.Values;
using MediatR;

namespace Folders.Application.UseCases.MoveFileToStorage;
public record MoveFileToStorageCommand(Core.Aggregates.File File, StorageProviderKey StorageProviderKey) : IRequest<Core.Aggregates.File>;
