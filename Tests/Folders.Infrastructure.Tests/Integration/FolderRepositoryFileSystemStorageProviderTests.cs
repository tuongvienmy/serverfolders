using DomainFundamentals;
using Folders.Application.Abstractions;
using Folders.Application.UseCases.AddFileToFolder;
using Folders.Application.UseCases.CreateRoot;
using Folders.Core.Aggregates;
using Folders.Core.Values;
using Folders.Infrastructure.Persistence;
using Folders.Infrastructure.Persistence.DatabaseContexts;
using Folders.Infrastructure.Storage;
using Folders.Infrastructure.Storage.StorageProviders.FileSystem;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using File = Folders.Core.Aggregates.File;

namespace Folders.Infrastructure.Tests.Integration;

[TestClass]
public class FolderRepositoryWithFileSystemStorageProviderTests
{
    private string _tempDir = null!;
    private FoldersDbContext _dbContext = null!;
    private FolderRepository _repository = null!;
    private FileSystemStorageProvider _storageProvider = null!; 

    private ServiceProvider _provider = null!;
    private IMediator _mediator = null!;
    private IServiceScope _scope = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        var services = new ServiceCollection();

        // Provide a minimal IHostEnvironment so MediatR.Licensing.LicenseAccessor can be constructed
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection().Build());

        // Provide logging and options used by MediatR internals (LicenseAccessor, etc.)
        services.AddLogging();
        services.AddOptions();

        // 1. EF Core DbContext (SQLite in-memory = relational semantics)
        services.AddDbContext<FoldersDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        // 2. Repository + UnitOfWork
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FoldersDbContext>());

        // 3. Storage system
        services.AddSingleton<IStorageProviderRegistry, StorageProviderRegistry>();
        services.AddScoped<IStorageManager, StorageManager>();

        // Register a real FileSystem provider (writes to TestFiles dir)
        var fileSystemProvider = new FileSystemStorageProvider(_tempDir, new DateBasedPathStrategy());
        services.AddSingleton<IStorageProvider>(fileSystemProvider);

        // 4. MediatR — auto-register handlers from Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AddFileToFolderHandler).Assembly);
        });

        _provider = services.BuildServiceProvider();

        // Ensure DB is created
        _scope = _provider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<FoldersDbContext>();
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.CloseConnection();
        _provider.Dispose();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }    

    [TestMethod]
    public async Task CreateFolder_And_AddFile_EndToEnd()
    {
        // 1. Create a folder via use case
        var createCmd = new CreateRootCommand("MyFolder");
        var folder = await _mediator.Send(createCmd);

        Assert.IsNotNull(folder);
        Assert.AreEqual("MyFolder", folder.Name);

        // 2. Add a file into the folder
        var fileBytes = Encoding.UTF8.GetBytes("Hello World");
        using var stream = new MemoryStream(fileBytes);

        var addFileCmd = new AddFileToFolderCommand(folder,"hello.txt",fileBytes, StorageProviderKey.File);

        var addedFile = await _mediator.Send(addFileCmd);

        Assert.IsNotNull(addedFile);
        Assert.AreEqual("hello.txt", addedFile.Name);

        // 3. Reload from DB to check persistence
        var reloadedFolder = await _dbContext.FolderItems.OfType<Folder>()
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == folder.Id);

        Assert.IsNotNull(reloadedFolder);
        Assert.IsTrue(reloadedFolder!.Items.OfType<File>().Any());

        var storedFile = reloadedFolder.Items.OfType<File>().Single();
        Assert.AreEqual("hello.txt", storedFile.Name);

        // 4. Verify file contents from storage provider
        var storageManager = _provider.GetRequiredService<IStorageManager>();
        using var retrieved = await storageManager.RetrieveStreamAsync(storedFile.StorageId);
        using var reader = new StreamReader(retrieved);
        var text = await reader.ReadToEndAsync();

        Assert.AreEqual("Hello World", text);
    }
}
