using CloudStorageMover.App.Pages;
using CloudStorageMover.App.Services;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Controls.Controls;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Services;

namespace CloudStorageMover.App;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly SourceSelectionPage _sourcePage;
    private readonly FileBrowserPage _fileBrowserPage;
    private readonly TransferPage _transferPage;
    private readonly ConfigurationPage _configPage;
    private readonly PhotoGalleryPage _galleryPage;
    private readonly CameraImportPage _cameraImportPage;
    private readonly AlbumManagerPage _albumManagerPage;
    private readonly SelectedFilesService _selectedFiles;
    private readonly GooglePhotosPickerService _photosPicker;
    private readonly DestinationSettingsService _destinationSettings;

    public MainPage(MainPageViewModel viewModel,
                    SourceSelectionPage sourcePage,
                    FileBrowserPage fileBrowserPage,
                    TransferPage transferPage,
                    ConfigurationPage configPage,
                    PhotoGalleryPage galleryPage,
                    CameraImportPage cameraImportPage,
                    AlbumManagerPage albumManagerPage,
                    SelectedFilesService selectedFiles,
                    GooglePhotosPickerService photosPicker,
                    DestinationSettingsService destinationSettings)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _sourcePage = sourcePage;
        _fileBrowserPage = fileBrowserPage;
        _transferPage = transferPage;
        _configPage = configPage;
        _galleryPage = galleryPage;
        _cameraImportPage = cameraImportPage;
        _albumManagerPage = albumManagerPage;
        _selectedFiles = selectedFiles;
        _photosPicker = photosPicker;
        _destinationSettings = destinationSettings;
        BindingContext = viewModel;

        MenuBar.ItemSelected += OnMenuItemSelected;
        MenuBar.QuitRequested += OnQuitRequested;
        MenuBar.PhotosPickerRequested += OnPhotosPickerRequested;
        MenuBar.GalleryRequested += OnGalleryRequested;
        MenuBar.AzurePhotosRequested += OnAzurePhotosRequested;
        MenuBar.CameraImportRequested += OnCameraImportRequested;
        _sourcePage.FoldersLoaded += OnSourceFoldersLoaded;
        _sourcePage.ProviderChanging += OnProviderChanging;
        _sourcePage.TransferRequested += OnTransferRequested;
        _fileBrowserPage.NavigateToTransferRequested += OnNavigateToTransferRequested;
        _fileBrowserPage.FolderSelected += OnFolderSelected;
        _galleryPage.BackRequested += OnGalleryBackRequested;
        _cameraImportPage.BackRequested += OnCameraImportBackRequested;
        _transferPage.BackRequested += OnTransferBackRequested;

        SourceSelectionArea.Content = _sourcePage;
    }

    private void OnMenuItemSelected(object? sender, AppMenuItem e)
    {
        _viewModel.NavigateTo(e);

        if (e == AppMenuItem.Configuration && ConfigArea.Content == null)
            ConfigArea.Content = _configPage;

        if (e != AppMenuItem.Configuration && ConfigArea.Content is ConfigurationPage configPage)
        {
            if (configPage.BindingContext is ConfigurationViewModel vm)
            {
                vm.IsAzureConnectionStringVisible = false;
                vm.IsMegaKeyVisible = false;
            }
        }

        if (e == AppMenuItem.Transfer)
        {
            if (TransferArea.Content == null)
                TransferArea.Content = _transferPage;
            _transferPage.Refresh();
        }
    }

    private void OnQuitRequested(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
    }

    private void OnSourceFoldersLoaded(object? sender, List<FileItem> folders)
    {
        var providerName = _sourcePage.BindingContext is SourceSelectionViewModel svm
            ? svm.SelectedProviderName
            : "Source";

        _viewModel.SetFolders(folders, providerName);
    }

    private async void OnFolderSelected(object? sender, FileItem folder)
    {
        if (_sourcePage.BindingContext is not SourceSelectionViewModel svm)
            return;

        var provider = svm.SelectedProvider;
        if (provider == null) return;

        await _fileBrowserPage.LoadFolderAsync(provider, folder);

        _viewModel.CurrentFolderName = folder.Name;
        _viewModel.ShowFileBrowser();
        FileBrowserArea.Content = _fileBrowserPage;
    }

    private async void OnProviderChanging(object? sender, StorageProviderType newType)
    {
        if (_selectedFiles.Count > 0)
        {
            var proceed = await DisplayAlert("Change Source",
                "Changing the source will clear the selected files list. Continue?",
                "Yes", "No");
            if (!proceed)
                return;
            _selectedFiles.Clear();
        }

        _viewModel.HideFileBrowser();
    }

    private void OnNavigateToTransferRequested()
    {
        OnMenuItemSelected(this, AppMenuItem.Transfer);
    }

    private void OnTransferRequested(object? sender, EventArgs e)
    {
        OnMenuItemSelected(this, AppMenuItem.Transfer);
    }

    private void OnGalleryBackRequested(object? sender, EventArgs e)
    {
        _viewModel.HideGallery();
        _viewModel.NavigateTo(AppMenuItem.Source);
    }

    private void OnAzurePhotosRequested(object? sender, EventArgs e)
    {
        if (AzurePhotosArea.Content == null)
            AzurePhotosArea.Content = _albumManagerPage;

        _viewModel.ShowAzurePhotos();
    }

    private void OnCameraImportBackRequested(object? sender, EventArgs e)
    {
        _viewModel.HideCameraImport();
        _viewModel.NavigateTo(AppMenuItem.Source);
    }

    private void OnTransferBackRequested(object? sender, EventArgs e)
    {
        _viewModel.NavigateTo(AppMenuItem.Source);
    }

    private async void OnGalleryRequested(object? sender, EventArgs e)
    {
        var downloadFolder = _destinationSettings.GooglePhotosDownloadPath;
        if (string.IsNullOrEmpty(downloadFolder) || !Directory.Exists(downloadFolder))
        {
            await DisplayAlert("Photo Gallery",
                "No Google Photos download folder configured.\n\nGo to Configuration (gear icon) to set the download folder path.",
                "OK");
            return;
        }

        if (GalleryArea.Content == null)
            GalleryArea.Content = _galleryPage;

        _galleryPage.LoadPhotos(downloadFolder);
        _viewModel.ShowGallery();
    }

    private void OnCameraImportRequested(object? sender, EventArgs e)
    {
        if (CameraImportArea.Content == null)
            CameraImportArea.Content = _cameraImportPage;

        _cameraImportPage.RefreshDevices();
        _viewModel.ShowCameraImport();
    }

    private async void OnPhotosPickerRequested(object? sender, EventArgs e)
    {
        var downloadFolder = _destinationSettings.GooglePhotosDownloadPath;
        if (string.IsNullOrEmpty(downloadFolder) || !Directory.Exists(downloadFolder))
        {
            try
            {
                Directory.CreateDirectory(downloadFolder);
            }
            catch
            {
                downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
        }

        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        PickerFlowResult result;

        try
        {
            result = await _photosPicker.RunPickerAsync(downloadFolder, cts.Token);
        }
        catch (OperationCanceledException)
        {
            result = PickerFlowResult.Failed("Picker timed out.");
        }
        catch (Exception ex)
        {
            result = PickerFlowResult.Failed($"Error: {ex.Message}");
        }

        if (result.IsSuccess && result.DownloadedCount > 0)
        {
            if (GalleryArea.Content == null)
                GalleryArea.Content = _galleryPage;

            _galleryPage.LoadPhotos(downloadFolder);
            _viewModel.ShowGallery();
        }
        else
        {
            await DisplayAlert("Google Photos Picker", result.Message, "OK");
        }
    }
}
