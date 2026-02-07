using Folders.UI.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Folders.UI.Components.Config;

public class FolderOptions
{
    #region Earlier version
    // Capacities
    public bool AllowNavigationUp { get; set; } = true;
    public bool AllowNavigationDown { get; set; } = true;
    public bool AllowAddFolders { get; set; } = true;
    public bool AllowAddFiles { get; set; } = true;
    public bool AllowSearch { get; set; } = true;
    public bool AllowSelect { get; set; } = true;

    // Events
    public EventCallback<FolderViewModel> OnFolderSelected { get; set; }
    public EventCallback<FileViewModel> OnFileSelected { get; set; }
    public EventCallback<(FolderViewModel Parent, string Name)> OnAddFolder { get; set; }
    public EventCallback<(FolderViewModel Parent, string Name)> OnAddFile { get; set; }

    // File display options
    public bool ShowMimeIcon { get; set; } = true;
    public bool ShowStorageProviderColor { get; set; } = true;
    public bool ShowTooltip { get; set; } = true;

    // Root initialization UI (when RootNode == null)
    public bool EnableRootInitialization { get; set; } = true;
    public bool ShowCreateButton { get; set; } = true;
    public bool ShowRetrieveButton { get; set; } = true;
    public string InitPlaceholder { get; set; } = "Enter folder name…";
    public RootInitAction EnterKeyAction { get; set; } = RootInitAction.Create;

    // Callbacks parent must handle (external state)
    public EventCallback<string> OnCreateRootRequested { get; set; }
    public EventCallback<string> OnRetrieveRootRequested { get; set; }

    // Optional validation hook
    public Func<string, bool>? ValidateRootName { get; set; }
    #endregion

    #region Current version
    /// <summary>
    /// Enable the search input inside the folder control.
    /// </summary>
    //public bool AllowSearch { get; set; } = true;

    /// <summary>
    /// Allow creating subfolders.
    /// </summary>
    public bool AllowAddFolder { get; set; } = true;

    /// <summary>
    /// Allow uploading files.
    /// </summary>
    public bool AllowUpload { get; set; } = true;

    /// <summary>
    /// Allow downloading files.
    /// </summary>
    public bool AllowDownload { get; set; } = true;

    /// <summary>
    /// Allow moving files between storage providers.
    /// </summary>
    public bool AllowMove { get; set; } = false;

    /// <summary>
    /// Optional: a curated list of provider keys to present in the UI.
    /// If null or empty, UI can fall back to a sensible default like { "s3", "file", "memory" }.
    /// </summary>
    public IList<string> AllowedProviders { get; set; } = new List<string> { "s3", "file", "mem" };

    /// <summary>
    /// Optional: whether to show hidden/system files in the UI.
    /// </summary>
    public bool ShowHiddenFiles { get; set; } = false;

    #endregion
}

public enum RootInitAction { Create, Retrieve }




