
using Folders.Core.Aggregates;
using Folders.Core.Values;
using Folders.Core;
using Folders.Infrastructure.Persistence.DatabaseContexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tests.Folders.Core;
using Folders.Infrastructure.Persistence;
using File = Folders.Core.Aggregates.File;

namespace Tests.Folders.Infrastructure;

[TestClass]
public class FolderIntegrationTestsSqlite
{    
    private SqliteConnection _connection;
    private DbContextOptions<FolderDbContext> _contextOptions;

    [TestInitialize]
    public void Setup()
    {
        //_connection = new SqliteConnection("DataSource=:memory:");
        _connection = new SqliteConnection("Data Source=FolderItemsDb.db;");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<FolderDbContext>()
            .UseSqlite(_connection)
            .Options;        
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connection.Close();
    }

    [TestMethod]
    public async Task FullFolderUseCase_WithSQLite_WorksCorrectly()
    {

        using (var context = new FolderDbContext(_contextOptions))
        {
            var root = Folder.NewFolder("root");
            var reports = root.AddFolder("Reports");

            var fileData = new byte[] { 1, 2, 3, 4, 5 };
            var file = reports.AddFile("report1.pdf", MimeType.FromFileName("report1.pdf"), fileData, FoldersTests.storage);

            file.Rename("report-renamed.pdf");
            file.SoftDelete();
            file.Restore();

            context.FolderItems.Add(root);
            await context.SaveChangesAsync();


            var loadedRoot = await context.FolderItems
                .OfType<Folder>()
                .Include(f => f.ParentFolder) // Include parent folder
                .Include(f => f.Items)
                .FirstOrDefaultAsync(f => f.Name == "root");            


            Assert.IsNotNull(loadedRoot);

            var loadedReports = loadedRoot.Items.OfType<Folder>().FirstOrDefault(f => f.Name == "Reports");
            Assert.IsNotNull(loadedReports);

            var loadedFile = loadedReports.Items.OfType<File>().FirstOrDefault(f => f.Name == "report-renamed.pdf");
            Assert.IsNotNull(loadedFile);
            Assert.AreEqual("report-renamed.pdf", loadedFile.Name);
            Assert.IsFalse(loadedFile.IsDeleted);
            Assert.AreEqual(5, loadedFile.Size);
            Assert.IsNotNull(loadedFile.StorageId);
        }        
    }
}