using Amazon.S3.Model;
using Folders.API.Models;
using Folders.Contracts;
using Folders.Core.Aggregates;

namespace Folders.API.Mappers;

public static class FolderItemMapper
{
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
