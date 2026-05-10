using Folders.Core.Aggregates;

namespace Folders.Application.DTOs;
public static class FolderItemMappers
{
    public static FolderDto ToPathDto(this Folder folder, IList<string> segments, int depth = 0)
    {
        // Determine if we are still navigating the path or if we've reached the target
        bool isStillOnPath = depth < segments.Count - 1;

        IEnumerable<FolderItem> itemsToMap;

        if (isStillOnPath)
        {
            // Only map the child that matches the NEXT segment in the path
            var nextSegment = segments[depth + 1];
            itemsToMap = folder.Items
                .Where(i => i.Name.Equals(nextSegment, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // We reached the last segment (end of the path), map ALL children normally
            itemsToMap = folder.Items;
        }

        return new FolderDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            folder.CreatedAt,
            folder.ModifiedAt,
            itemsToMap.Select(item =>
            {
                // If we are still navigating, use the path-aware mapper recursively
                if (isStillOnPath && item is Folder subFolder)
                    return subFolder.ToPathDto(segments, depth + 1);

                // Otherwise, use the standard recursive mapper for descendants
                return item.ToDto();
            }).ToList());
    }

    public static FolderDto ToDto(this Folder folder) =>
         new FolderDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            folder.CreatedAt,
            folder.ModifiedAt,
            folder.Items.Select(MapItem).ToList());


    private static FolderItemDto MapItem(FolderItem item) =>
        item switch
        {
            Core.Aggregates.File f => f.ToDto(),
            Folder sub => sub.ToDto(), // recursive call
            _ => throw new NotSupportedException($"Unknown item type {item.GetType().Name}")
        };

    public static FileDto ToDto(this Core.Aggregates.File file) =>
        new FileDto(
            file.Id,
            file.Name,
            file.ParentFolderId,
            file.CreatedAt,
            file.ModifiedAt,
            file.StorageId.Provider,
            file.StorageId.Value, // unwrap VO to string
            file.MimeType.Value,  // unwrap VO to string
            file.Size
        );
    public static FolderItemDto ToDto(this FolderItem item) => MapItem(item);
}
