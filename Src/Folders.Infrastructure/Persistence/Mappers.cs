using Folders.Core;
using Folders.Core.Entities;
using Folders.Core.Repositories;
using Folders.Infrastructure.Persistence.Entities;
using File = Folders.Core.Entities.File;

internal static class Mappers;
public static class FolderItemMapper
{
    // Map database entity to domain entity
    public static FolderItem ToDomain(this IFolderItemEntity entity)
    {
        if (entity is IFolderEntity folderEntity)
        {
            return new Folder(folderEntity);            
        }
        else if (entity is IFileEntity fileEntity)
        {
            return new File(fileEntity);
        }
        throw new InvalidOperationException("Unknown IFolderItemEntity type");
    }    
    public static IFolderItemEntity ToEntity(this FolderItem item)
    {
        if (item is File file)
        {
            return new FileEntity
            {
                Name = file.Name,
                ParentId = file.Parent?.ItemId,
                Metadata = file.Metadata,
                StorageId = file.StorageId.ToString(),
                Size = file.Size,
                MimeType = file.MimeType.ToString(),
                IsDeleted = file.IsDeleted,
                CreatedAt = file.CreatedAt,
                ModifiedAt = file.ModifiedAt
            };
        }
        if (item is Folder folder)
        {            
            var entity = new FolderEntity
            {
                Name = folder.Name,
                ParentId = folder.Parent?.ItemId,
                Metadata = folder.Metadata,
                IsDeleted = folder.IsDeleted,
                CreatedAt = folder.CreatedAt,
                ModifiedAt = folder.ModifiedAt,                                
            };
            foreach (var subItem in folder.Items)
            {
                if (subItem is File f)
                    entity.Items.Add(new FileEntity()
                    {
                        Name = f.Name,
                        ParentId = f.Parent?.ItemId,
                        Metadata = f.Metadata,
                        StorageId = f.StorageId.ToString(),
                        Size = f.Size,
                        MimeType = f.MimeType.ToString(),
                        IsDeleted = f.IsDeleted,
                        CreatedAt = f.CreatedAt,
                        ModifiedAt = f.ModifiedAt
                    });
                else if (subItem is Folder subFolder)
                    entity.Items.Add(new FolderEntity()
                    {
                        Name = subFolder.Name,
                        ParentId = subFolder.Parent?.ItemId,
                        Metadata = subFolder.Metadata,
                        IsDeleted = subFolder.IsDeleted,
                        CreatedAt = subFolder.CreatedAt,
                        ModifiedAt = subFolder.ModifiedAt
                    });
                else
                    throw new InvalidOperationException("Unknown FolderItem type.");
            }
            return entity;

        }

        throw new InvalidOperationException("Unknown FolderItem type.");
    }    
}
