using Folders.UI.Components.Config;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Folders.UI.ViewModels;

namespace Folders.UI.Pages;

public partial class Folders : ComponentBase
{
    private List<FolderViewModel> _folders = new();
    private FolderViewModel? _pendingParent;
    private FolderOptions? _folderOptions;
    private InputFile? _fileInput;
    private IBrowserFile? _pendingFile;
    private string _searchText = string.Empty;

    private IEnumerable<FolderViewModel> FilteredFolders =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _folders
            : _folders.Where(f => FolderOrDescendantMatches(f, _searchText));

    private bool FolderOrDescendantMatches(FolderViewModel folder, string search)
    {
        if (folder.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            return true;
        if (folder.Folders != null && folder.Folders.Any(child => FolderOrDescendantMatches(child, search)))
            return true;
        if (folder.Files != null && folder.Files.Any(file => file.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
            return true;
        return false;
    }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private services.FoldersApiClient client { get; set; } = default!;

    protected override void OnInitialized()
    {
        _folderOptions = new FolderOptions
        {
            EnableRootInitialization = true,
            AllowNavigationDown = true,
            ShowCreateButton = true,
            ShowRetrieveButton = true,
            EnterKeyAction = RootInitAction.Create,
            OnCreateRootRequested = EventCallback.Factory.Create<string>(this, CreateRootAsync),
            OnRetrieveRootRequested = EventCallback.Factory.Create<string>(this, RetrieveRootAsync),
            //OnAddFolder = EventCallback.Factory.Create<(FolderViewModel, string)>(this, AddFolderAsync),
            //OnAddFile = EventCallback.Factory.Create<(FolderViewModel, string)>(this, TriggerFileUploadAsync),            
        };
    }

    private async Task CreateRootAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var folder = await client.NewRoot(Uri.EscapeDataString(name));
        if (folder != null)
        {
            _folders.Insert(0, new FolderViewModel(folder));
        }
        StateHasChanged();
    }

    private async Task RetrieveRootAsync(string name)
    {
        var folders = await client.GetFolderByNameAsync(name);
        if (folders == null || folders.Count == 0)
            return;

        _folders = folders.Select(dto => new FolderViewModel(dto)).ToList();
        StateHasChanged();
    }

    private async Task AddFolderAsync((FolderViewModel Parent, string Name) args)
    {
        //var dto = await client.AddFolderAsync(args.Parent.Id, args.Name);
        //args.Parent.Folders.Add(new FolderViewModel(dto));
        //StateHasChanged();
    }

    private async Task TriggerFileUploadAsync((FolderViewModel Parent, string Name) args)
    {
        _pendingParent = args.Parent;
        await JS.InvokeVoidAsync("fileInterop.triggerFileInput", "hiddenFileInput");
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        if (_pendingParent is null) return;

        var file = e.File;
        var dto = await client.UploadFileAsync(_pendingParent.Id, file);
        var newFile = new FileViewModel(dto);

        _pendingParent.Files.Add(newFile);

        _pendingParent = null;
        //StateHasChanged();
    }
}