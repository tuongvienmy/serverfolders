using MediatR;

namespace Folders.Core.Events;

public class FolderItemRestoredDomainEvent: INotification
{
    public Guid FolderItemId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public FolderItemRestoredDomainEvent(Guid folderItemId)
    {
        FolderItemId = folderItemId;
    }
}
