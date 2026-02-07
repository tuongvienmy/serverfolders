using DomainFundamentals;
using Folders.Core.Aggregates;
using Folders.Core.Values;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Text.Json;

namespace Folders.Infrastructure.Persistence.DatabaseContexts;
public class FoldersDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator? _mediator;

    public DbSet<FolderItem> FolderItems { get; set; }

    public FoldersDbContext(DbContextOptions<FoldersDbContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;
    }    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var mimeTypeConverter = new ValueConverter<MimeType, string>(
            v => v.Value,
            v => v);

        var storageIdConverter = new ValueConverter<StorageId, string>(
            v => v.Value,
            v => v);

        var dictionaryConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions()) ?? new());


        // --- FolderItem base ---
        modelBuilder.Entity<FolderItem>(entity =>
        {                
            entity.ToTable("FolderItems")
                  .HasDiscriminator<string>("FolderItemType")                  
                  .HasValue<Folder>("Folder")
                  .HasValue<Core.Aggregates.File>("File");            

            entity.Property<string>("FolderItemType")
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(f => f.Id)
                  .HasValueGenerator<SequentialGuidValueGenerator>()
                  .ValueGeneratedOnAdd();

            entity.Property(f => f.Name).IsRequired();
            entity.Property(f => f.CreatedAt).IsRequired();
            entity.Property(f => f.ModifiedAt).IsRequired();
            entity.Property(f => f.IsDeleted).IsRequired();

            entity.HasOne(f => f.ParentFolder)
                  .WithMany(f => f.Items)
                  .HasForeignKey(f => f.ParentFolderId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);

            entity.Property(f => f.Metadata)
                  .HasConversion(dictionaryConverter)
                  .HasColumnType(Database.IsSqlite() ? "TEXT" : "nvarchar(max)")
                  .IsRequired(false);
        });       

        // --- File-specific ---
        modelBuilder.Entity<Core.Aggregates.File>(file =>
        {
            file.OwnsOne(f => f.StorageInfo, storage =>
            {
                storage.Property(s => s.Size)
                       .HasColumnName("Size");

                storage.Property(s => s.StorageId)
                       .HasConversion(storageIdConverter)
                       .HasColumnName("StorageId")
                       .HasMaxLength(250)
                       .HasColumnType("nvarchar(250)")
                       .IsRequired();

                storage.Property(s => s.MimeType)
                       .HasConversion(mimeTypeConverter)
                       .HasColumnName("MimeType")
                       .HasMaxLength(50)
                       .HasColumnType(Database.IsSqlite() ? "TEXT" : "nvarchar(50)")
                       .IsRequired();
            });
        });

    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Save changes to the database
        await base.SaveChangesAsync(cancellationToken);

        if (_mediator is null)
            return true;

        // Dispatch domain events
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();                

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // Clear domain events
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        return true;
    }
}
