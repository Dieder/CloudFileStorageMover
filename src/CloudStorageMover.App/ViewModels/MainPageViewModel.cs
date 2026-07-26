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
    private bool _isCameraImportPageVisible;

    [ObservableProperty]
    private bool _isAzurePhotosPageVisible;

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
        IsCameraImportPageVisible = false;
        IsAzurePhotosPageVisible = false;
    }

    public void ShowGallery()
    {
        IsSourcePageVisible = false;
        IsTransferPageVisible = false;
        IsConfigPageVisible = false;
        IsGalleryPageVisible = true;
        IsCameraImportPageVisible = false;
    }

    public void ShowCameraImport()
    {
        IsSourcePageVisible = false;
        IsTransferPageVisible = false;
        IsConfigPageVisible = false;
        IsGalleryPageVisible = false;
        IsCameraImportPageVisible = true;
        IsAzurePhotosPageVisible = false;
    }

    public void ShowAzurePhotos()
    {
        IsSourcePageVisible = false;
        IsTransferPageVisible = false;
        IsConfigPageVisible = false;
        IsGalleryPageVisible = false;
        IsCameraImportPageVisible = false;
        IsAzurePhotosPageVisible = true;
    }

    public void HideGallery()
    {
        IsGalleryPageVisible = false;
    }

    public void HideCameraImport()
    {
        IsCameraImportPageVisible = false;
    }

    public void HideAzurePhotos()
    {
        IsAzurePhotosPageVisible = false;
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
