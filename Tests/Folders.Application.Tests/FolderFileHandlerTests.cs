using DomainFundamentals;
using Folders.Application.Abstractions;
using Folders.Application.Exceptions;
using Folders.Application.UseCases.AddFileToFolder;
using Folders.Application.UseCases.AddSubFolder;
using Folders.Application.UseCases.CreateRoot;
using Folders.Application.UseCases.RenameFolder;
using Folders.Core.Aggregates;
using Folders.Core.Values;
using NSubstitute;
using System.Reflection;


namespace Folders.Application.Tests;

[TestClass]
public sealed class FolderFileHandlerTests
{    
    private IFolderRepository? _folderRepo;
    private IStorageManager? _storageManager;
    private IUnitOfWork? _unitOfWork;
        
    [TestInitialize]
    public void Initialize()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _folderRepo = Substitute.For<IFolderRepository>();
        _folderRepo!.UnitOfWork.Returns(_unitOfWork);
        _storageManager = Substitute.For<IStorageManager>();

        // Track folders passed to AddAsync / UpdateAsync so SaveChanges can emulate EF and assign ids
        var tracked = new List<Folder>();

        _folderRepo.AddAsync(Arg.Any<Folder>()).Returns(Task.CompletedTask);
        _folderRepo.When(r => r.AddAsync(Arg.Any<Folder>()))
            .Do(callInfo =>
            {
                var f = callInfo.Arg<Folder>();
                if (!tracked.Contains(f))
                    tracked.Add(f);
            });

        _folderRepo.UpdateAsync(Arg.Any<Folder>()).Returns(Task.CompletedTask);
        _folderRepo.When(r => r.UpdateAsync(Arg.Any<Folder>()))
            .Do(callInfo =>
            {
                var f = callInfo.Arg<Folder>();
                if (!tracked.Contains(f))
                    tracked.Add(f);
            });

        // Emulate EF: on SaveChangesAsync assign Ids to tracked folders and their children (files & subfolders)
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(callInfo =>
        {
            foreach (var f in tracked.ToArray())
                AssignIdsRecursively(f);
            return Task.FromResult(1);
        });

        _storageManager = Substitute.For<IStorageManager>();
        
    }
    private static void AssignIdsRecursively(Folder folder)
    {
        EnsureId(folder);

        // Items is IReadOnlyCollection<FolderItem>
        foreach (var item in folder.Items.ToList())
        {
            // ensure child's ParentFolderId points to parent
            var parentIdProp = item.GetType().GetProperty("ParentFolderId", BindingFlags.Public | BindingFlags.Instance);
            if (parentIdProp != null)
                parentIdProp.SetValue(item, folder.Id);

            EnsureId(item);

            if (item is Folder subFolder)
            {
                // recurse into subfolders
                AssignIdsRecursively(subFolder);
            }
        }
    }
    private static void EnsureId(object entity)
    {
        if (entity == null) return;
        var idProp = entity.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp == null) return;

        var current = idProp.GetValue(entity);
        if (current is Guid g && g != Guid.Empty)
            return;

        // Id setter is protected; retrieve non-public setter and invoke
        var setMethod = idProp.GetSetMethod(true);
        setMethod?.Invoke(entity, new object[] { Guid.NewGuid() });
    }

    [TestMethod]
    public async Task CreateNewFolder_ShoudCreateRootFolder()
    {
        var command = new CreateRootCommand("Test Folder");
        var handler = new CreateRootCommandHandler(_folderRepo!);
        var folder = await handler.Handle(command, CancellationToken.None);
        
        await _folderRepo!.Received(1).AddAsync(folder);
        await _folderRepo!.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.IsNotNull(folder);
        Assert.AreNotEqual(folder.Id, Guid.Empty);
        Assert.AreEqual("Test Folder",folder.Name);
        Assert.AreEqual(0, folder.NumberOfItems);
        Assert.IsNull(folder.ParentFolder);
    }

    [TestMethod]
    public async Task RenameFolder_ShouldRename_WhenFolderExist()
    {
        var command = new CreateRootCommand("Test Folder");
        var handler = new CreateRootCommandHandler(_folderRepo!);
        var folder = await handler.Handle(command, CancellationToken.None);

        Assert.IsNotNull(folder);
        Assert.AreNotEqual(folder.Id, Guid.Empty);
        Assert.AreEqual("Test Folder", folder.Name);
        Assert.AreEqual(0,folder.NumberOfItems);
        Assert.IsNull(folder.ParentFolder);
        
        var renameCommand = new RenameFolderItemCommand(folder.Id, "Renamed Folder");
        var renameHandler = new RenameFolderItemHandler(_folderRepo!);
        _folderRepo!.GetByIdAsync(folder.Id)!.Returns(Task.FromResult(folder));

        var renamedFolder = await renameHandler.Handle(renameCommand, CancellationToken.None);
        await _folderRepo!.Received(1).AddAsync(folder);
        await _folderRepo!.Received(1).GetByIdAsync(folder.Id);
        await _folderRepo!.Received(1).UpdateAsync(folder);
        await _folderRepo!.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [TestMethod]
    public async Task RenameFolder_ShouldThrowException_WhenFolderNotFound()
    {
        var command = new RenameFolderItemCommand(Guid.NewGuid(), "Renamed Folder");
        var renameHandler = new RenameFolderItemHandler(_folderRepo!);
        _folderRepo!.GetByIdAsync(command.FolderId).Returns(Task.FromResult<Folder?>(null));
        await Assert.ThrowsExactlyAsync<FolderNotFoundException>(() => renameHandler.Handle(command, CancellationToken.None));
        
        await _folderRepo!.Received(1).GetByIdAsync(command.FolderId);
        await _folderRepo!.Received(0).UpdateAsync(Arg.Any<Folder>());
        await _folderRepo!.UnitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task AddFileToFolderHandler_ShouldAddFileToFolder_UpdateAndCommit_ReturnFileWithCorrectStorageInfo()
    {
        var createCommand = new CreateRootCommand("Test Folder");
        var createHandler = new CreateRootCommandHandler(_folderRepo!);
        var folder = await createHandler.Handle(createCommand, CancellationToken.None);

        var data = new byte[] {1,2,3};

        var storageInfo = new StorageInfo(new StorageId(StorageProviderKey.Memory, "Files", "/file1.pdf"), MimeType.Pdf, data.LongLength);
        _storageManager!.StoreAsync(data,StorageProviderKey.Memory).Returns(Task.FromResult(storageInfo));

        var command = new AddFileToFolderCommand(folder, "file1.pdf", data, StorageProviderKey.Memory);

        var handler = new AddFileToFolderHandler(_folderRepo!, _storageManager);
        var file = await handler.Handle(command,CancellationToken.None);

        await _folderRepo!.Received(0).GetByIdAsync(folder.Id);
        await _folderRepo!.Received(1).UpdateAsync(folder);
        await _folderRepo!.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.AreNotEqual(folder.Id, Guid.Empty);
        Assert.AreEqual("Test Folder", folder.Name);
        Assert.AreEqual(1, folder.NumberOfItems); 
        Assert.IsNotNull(file);
        Assert.AreNotEqual(file.Id,Guid.Empty);
        Assert.AreEqual("file1.pdf", file.Name);
        Assert.AreEqual(MimeType.Pdf, file.MimeType);
        Assert.AreEqual(data.LongLength, file.Size);
        Assert.IsTrue(file.ParentFolder is not null && file.ParentFolder == folder);

    }
    [TestMethod]
    public async Task AddSubFolderHandler_ShouldAddSubFolder_WhenSubFolderNameAvailable()
    {
        var command = new CreateRootCommand("Test Folder");
        var handler = new CreateRootCommandHandler(_folderRepo!);
        var root = await handler.Handle(command, CancellationToken.None);
        Assert.IsNotNull(root);
        Assert.AreNotEqual(root.Id, Guid.Empty);
        Assert.AreEqual("Test Folder", root.Name);
        Assert.AreEqual(0, root.NumberOfItems);
        Assert.IsNull(root.ParentFolder);

        var subCommand = new AddSubFolderCommand(root.Id, "SubFolder1");
        var subHandler = new AddSubFolderHandler(_folderRepo!);
        
        _folderRepo!.GetByIdAsync(root.Id).Returns(Task.FromResult(root as Folder));
        
        var subFolder = await subHandler.Handle(subCommand, CancellationToken.None);

        await _folderRepo!.Received(1).GetByIdAsync(root.Id);
        await _folderRepo!.Received(1).UpdateAsync(root);
        await _folderRepo!.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.IsNotNull(subFolder);
        Assert.AreNotEqual(subFolder.Id, Guid.Empty);
        Assert.AreEqual("SubFolder1", subFolder.Name);
    }
    [TestMethod]
    public async Task AddSubFolderHandler_ShouldAppendCounterInParenthesis_WhenSubFolderNameAlreadyExists()
    {
        var command = new CreateRootCommand("Test Folder");
        var handler = new CreateRootCommandHandler(_folderRepo!);
        var root = await handler.Handle(command, CancellationToken.None);
        Assert.IsNotNull(root);
        Assert.AreNotEqual(root.Id, Guid.Empty);
        Assert.AreEqual("Test Folder", root.Name);
        Assert.AreEqual(0, root.NumberOfItems);
        Assert.IsNull(root.ParentFolder);

        _folderRepo!.GetByIdAsync(root.Id).Returns(Task.FromResult(root as Folder));

        var subCommand = new AddSubFolderCommand(root.Id, "SubFolder1");
        var subHandler = new AddSubFolderHandler(_folderRepo!);
        var subFolder1 = await subHandler.Handle(subCommand, CancellationToken.None);
        
        var subFolder2 = await subHandler.Handle(subCommand, CancellationToken.None);

        await _folderRepo!.Received(2).GetByIdAsync(root.Id);
        await _folderRepo!.Received(2).UpdateAsync(root);
        await _folderRepo!.UnitOfWork.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.IsNotNull(subFolder2);
        Assert.AreNotEqual(subFolder2.Id, Guid.Empty);
        Assert.AreEqual("SubFolder1 (1)", subFolder2.Name);
        Assert.AreEqual(2, root.NumberOfItems);

    }

    [TestMethod]
    public async Task AddSubFolderHandler_ShouldThrowException_WhenParentFolderNotFound()
    {
        var command = new AddSubFolderCommand(Guid.NewGuid(), "SubFolder1");
        var subHandler = new AddSubFolderHandler(_folderRepo!);
        _folderRepo!.GetByIdAsync(command.parentFolderId).Returns(Task.FromResult<Folder?>(null));

        await Assert.ThrowsExactlyAsync<FolderNotFoundException>(() => subHandler.Handle(command, CancellationToken.None));
        
        await _folderRepo!.Received(1).GetByIdAsync(command.parentFolderId);
        await _folderRepo!.Received(0).UpdateAsync(Arg.Any<Folder>());
        await _folderRepo!.UnitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}