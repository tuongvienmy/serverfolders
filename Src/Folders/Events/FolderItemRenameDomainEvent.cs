using MediatR;

namespace Folders.Core.Events;

public class FolderItemRenameDomainEvent: INotification
{
    public Guid FolderItemId { get; }
    public string OldName { get; }
    public string NewName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public FolderItemRenameDomainEvent(Guid folderItemId, string oldName, string newName)
    {
        FolderItemId = folderItemId;
        NewName = newName;
        OldName = oldName;
    }
}
