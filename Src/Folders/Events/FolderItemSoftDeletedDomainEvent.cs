using MediatR;

namespace Folders.Core.Events;

public class FolderItemSoftDeletedDomainEvent: INotification
{
    public Guid FolderItemId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public FolderItemSoftDeletedDomainEvent(Guid folderItemId)
    {
        FolderItemId = folderItemId;
    }
}
