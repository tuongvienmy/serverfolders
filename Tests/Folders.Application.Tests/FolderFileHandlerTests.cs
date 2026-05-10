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

        var rootFolder = Folder.CreateRoot("Test Folder");

        _folderRepo!.GetByIdAsync(rootFolder.Id).Returns(rootFolder);
        _folderRepo!.UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var handler = new CreateRootCommandHandler(_folderRepo!);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, rootFolder.Id);
        Assert.AreEqual("Test Folder",result.Name);
        Assert.AreEqual(0, result.Items.Count);
        Assert.IsNull(result.ParentId);
    }

    [TestMethod]
    public async Task RenameFolder_ShouldRename_WhenFolderExist()
    {
        var root = Folder.CreateRoot("Root");
        var folder = root.AddFolder("Test Folder");
        
        var renameCommand = new RenameFolderItemCommand(folder.Id, "Renamed Folder");
        var renameHandler = new RenameFolderItemHandler(_folderRepo!);
        _folderRepo!.GetByIdAsync(folder.Id)!.Returns(folder);

        var renamedFolder = await renameHandler.Handle(renameCommand, CancellationToken.None);

        await _folderRepo!.Received(1).UpdateAsync(folder);
        _folderRepo!.UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
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
        var root = Folder.CreateRoot("Root");
        var folder = root.AddFolder("Test Folder");

        var data = new byte[] {1,2,3};

        var storageInfo = new StorageInfo(new StorageId(StorageProviderKey.Memory, "Files", "/file1.pdf"), MimeType.Pdf, data.LongLength);
        //_storageManager!.StoreStreamAsync(new MemoryStream(data), StorageProviderKey.Memory).Returns(await Task.FromResult(storageInfo));
        _storageManager!.StoreStreamAsync(Arg.Any<Stream>(), StorageProviderKey.Memory).Returns(Task.FromResult(storageInfo));
        _folderRepo!.GetByIdAsync(folder.Id).Returns(Task.FromResult<Folder?>(folder));

        var command = new AddFileToFolderCommand(folder.Id, "file1.pdf", new MemoryStream(data), StorageProviderKey.Memory);

        var handler = new AddFileToFolderHandler(_folderRepo!, _storageManager!);

        var file = await handler.Handle(command,CancellationToken.None);

        await _folderRepo!.Received(0).GetByIdAsync(folder.Id);
        await _folderRepo!.Received(1).UpdateAsync(folder);
        await _unitOfWork!.Received().SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.AreNotEqual(folder.Id, Guid.Empty);
        Assert.AreEqual("Test Folder", folder.Name);
        Assert.AreEqual(1, folder.NumberOfItems); 
        Assert.IsNotNull(file);
        Assert.AreNotEqual(file.Id,Guid.Empty);
        Assert.AreEqual("file1.pdf", file.Name);
        Assert.AreEqual(MimeType.Pdf.ToString(), file.MimeType);
        Assert.AreEqual(data.LongLength, file.Size);
        Assert.IsTrue(file.ParentId == folder.Id);

    }
    [TestMethod]
    public async Task AddSubFolderHandler_ShouldAddSubFolder_WhenSubFolderNameAvailable()
    {
        var root = Folder.CreateRoot("Root");

        var subCommand = new AddSubFolderCommand(root.Id, "SubFolder1");
        var subHandler = new AddSubFolderHandler(_folderRepo!);
        
        _folderRepo!.GetByIdAsync(root.Id).Returns(root);
        
        var subFolder = await subHandler.Handle(subCommand, CancellationToken.None);

        await _folderRepo!.Received(1).UpdateAsync(root);
        await _unitOfWork!.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.IsNotNull(subFolder);
        Assert.AreNotEqual(subFolder.Id, Guid.Empty);
        Assert.AreEqual("SubFolder1", subFolder.Name);
    }
    [TestMethod]
    public async Task AddSubFolderHandler_ShouldAppendCounterInParenthesis_WhenSubFolderNameAlreadyExists()
    {
        var root = Folder.CreateRoot("Root");

        _folderRepo!.GetByIdAsync(root.Id).Returns(root);

        var subCommand = new AddSubFolderCommand(root.Id, "SubFolder1");

        var subHandler = new AddSubFolderHandler(_folderRepo!);
        var subFolder1 = await subHandler.Handle(subCommand, CancellationToken.None);
        
        var subFolder2 = await subHandler.Handle(subCommand, CancellationToken.None);

        await _folderRepo!.Received(2).UpdateAsync(root);
        await _folderRepo!.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());

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
        _folderRepo!.GetByIdAsync(command.parentFolderId).Returns((Folder?)null);

        await Assert.ThrowsExactlyAsync<FolderNotFoundException>(() => subHandler.Handle(command, CancellationToken.None));
        
        await _folderRepo!.Received(1).GetByIdAsync(command.parentFolderId);
        await _folderRepo!.Received(0).UpdateAsync(Arg.Any<Folder>());
        await _folderRepo!.UnitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}