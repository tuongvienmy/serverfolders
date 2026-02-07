namespace Folders.Infrastructure.Persistence.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class FolderDbContextFactory : IDesignTimeDbContextFactory<FoldersDbContext>
{
    public FoldersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FoldersDbContext>();

        // Use your test or dev connection string here
        //optionsBuilder.UseSqlite("Server=(localdb)\\mssqllocaldb;Database=FolderItemsDb;Trusted_Connection=True;");

        //optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


        optionsBuilder.UseSqlite("Data Source=FolderItemsDb.db;");

        return new FoldersDbContext(optionsBuilder.Options);
    }
}

