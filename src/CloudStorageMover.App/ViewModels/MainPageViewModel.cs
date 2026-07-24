using CommunityToolkit.Mvvm.ComponentModel;
using CloudStorageMover.Controls.Controls;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private AppMenuItem _selectedMenuItem = AppMenuItem.Source;

    [ObservableProperty]
    private bool _isSourcePageVisible = true;

    [ObservableProperty]
    private bool _isTransferPageVisible;

    [ObservableProperty]
    private bool _isConfigPageVisible;

    [ObservableProperty]
    private bool _isGalleryPageVisible;

    [ObservableProperty]
    private string _currentProviderName = string.Empty;

    [ObservableProperty]
    private bool _isFileBrowserVisible;

    [ObservableProperty]
    private string _currentFolderName = string.Empty;

    [ObservableProperty]
    private int _selectedFilesCount;

    [ObservableProperty]
    private string _selectedFilesSummary = string.Empty;

    public void NavigateTo(AppMenuItem item)
    {
        SelectedMenuItem = item;
        IsSourcePageVisible = item == AppMenuItem.Source;
        IsTransferPageVisible = item == AppMenuItem.Transfer;
        IsConfigPageVisible = item == AppMenuItem.Configuration;
        IsGalleryPageVisible = false;
    }

    public void ShowGallery()
    {
        IsSourcePageVisible = false;
        IsTransferPageVisible = false;
        IsConfigPageVisible = false;
        IsGalleryPageVisible = true;
    }

    public void HideGallery()
    {
        IsGalleryPageVisible = false;
    }

    public void SetFolders(List<FileItem> folders, string providerName)
    {
        CurrentProviderName = providerName;
        IsSourcePageVisible = true;
        IsFileBrowserVisible = false;
    }

    public void ShowFileBrowser()
    {
        IsFileBrowserVisible = true;
    }

    public void HideFileBrowser()
    {
        IsFileBrowserVisible = false;
    }
}
