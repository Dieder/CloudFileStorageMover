#if WINDOWS
using MediaDevices;
#endif
using CloudStorageMover.App.Services;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Core.Services;

namespace CloudStorageMover.App.Pages;

public partial class CameraImportPage : ContentView
{
    private readonly CameraImportViewModel _viewModel;
    private readonly DestinationSettingsService _destinationSettings;
#if WINDOWS
    private readonly CameraImportService _cameraService;
#endif

    public event EventHandler? BackRequested;

    public CameraImportPage(CameraImportViewModel viewModel,
                             DestinationSettingsService destinationSettings)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _destinationSettings = destinationSettings;
        BindingContext = viewModel;

#if WINDOWS
        _cameraService = new CameraImportService();
        viewModel.IsNotWindowsOnly = true;
        LoadDevices();
#else
        viewModel.IsWindowsOnly = true;
        viewModel.IsNotWindowsOnly = false;
#endif
    }

    public void RefreshDevices()
    {
#if WINDOWS
        LoadDevices();
#endif
    }

#if WINDOWS
    private void LoadDevices()
    {
        _viewModel.Devices.Clear();
        foreach (var device in _cameraService.GetDevices())
        {
            _viewModel.Devices.Add(new CameraDeviceItem
            {
                DeviceId = device.DeviceId,
                DisplayName = device.DisplayName,
                ConnectionType = device.ConnectionLabel
            });
        }
    }

    private void OnDeviceSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CameraDeviceItem selected)
            return;

        if (!_cameraService.Connect(selected.DeviceId))
        {
            _viewModel.StatusMessage = "Failed to connect to device.";
            return;
        }

        _viewModel.IsDeviceListVisible = false;
        _viewModel.IsPhotoListVisible = true;
        _viewModel.IsLoading = true;
        _viewModel.IsImportPreviewVisible = false;

        try
        {
            var photos = _cameraService.GetAllPhotos();
            _viewModel.Photos.Clear();
            foreach (var photo in photos)
            {
                _viewModel.Photos.Add(new CameraPhotoItem
                {
                    FileName = photo.FileName,
                    FilePath = photo.FilePath,
                    Size = photo.Size,
                    DateTaken = photo.DateTaken
                });
            }

            var outputFolder = _destinationSettings.GooglePhotosDownloadPath;
            if (!string.IsNullOrEmpty(outputFolder))
            {
                var existingFiles = _cameraService.GetExistingFiles(outputFolder);
                var toImport = _cameraService.CountPhotosToImport(photos, existingFiles);
                _viewModel.ShowImportPreview(photos.Count, toImport, photos.Count - toImport);
            }
            else
            {
                _viewModel.ShowImportPreview(photos.Count, photos.Count, 0);
            }

            _viewModel.StatusMessage = photos.Count == 0
                ? "No photos found on device."
                : string.Empty;
        }
        catch (Exception ex)
        {
            _viewModel.StatusMessage = $"Error reading photos: {ex.Message}";
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Photos.Count == 0)
            return;

        var outputFolder = _destinationSettings.GooglePhotosDownloadPath;
        if (string.IsNullOrEmpty(outputFolder))
        {
            _viewModel.StatusMessage = "No import folder configured. Set it in Configuration.";
            return;
        }

        Directory.CreateDirectory(outputFolder);

        if (_viewModel.ToImportCount == 0)
        {
            _viewModel.StatusMessage = "No new photos to import (all already exist).";
            return;
        }

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Import",
            $"Import {_viewModel.ToImportCount} new photo(s)?\n" +
            $"({_viewModel.AlreadyExistsCount} already exist and will be skipped)",
            "Import",
            "Cancel");

        if (!confirm)
            return;

        _viewModel.StartConfirmingImport();

        var deleteAfter = _viewModel.DeleteAfterImport;
        var imported = 0;
        var skipped = 0;
        var failed = 0;

        try
        {
            var existingFiles = _cameraService.GetExistingFiles(outputFolder);

            foreach (var photo in _viewModel.Photos.ToList())
            {
                var success = _cameraService.DownloadPhoto(
                    new CameraPhoto
                    {
                        FileName = photo.FileName,
                        FilePath = photo.FilePath,
                        Size = photo.Size,
                        DateTaken = photo.DateTaken
                    },
                    outputFolder,
                    deleteAfter,
                    existingFiles);

                if (success)
                    imported++;
                else
                    failed++;
            }

            _viewModel.FinishImport(imported, skipped, failed);
        }
        catch (Exception ex)
        {
            _viewModel.StatusMessage = $"Import error: {ex.Message}";
            _viewModel.IsLoading = false;
        }
        finally
        {
            _cameraService.Disconnect();
        }
    }
#else
    private void OnDeviceSelected(object? sender, SelectionChangedEventArgs e) { }
    private void OnImportClicked(object? sender, EventArgs e) { }
#endif

    private void OnScanClicked(object? sender, EventArgs e)
    {
        RefreshDevices();
    }

    private void OnBackToDevicesClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        _cameraService.Disconnect();
#endif
        _viewModel.IsDeviceListVisible = true;
        _viewModel.IsPhotoListVisible = false;
        _viewModel.Photos.Clear();
        _viewModel.StatusMessage = string.Empty;
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        _cameraService.Disconnect();
#endif
        _viewModel.IsDeviceListVisible = true;
        _viewModel.IsPhotoListVisible = false;
        _viewModel.Photos.Clear();
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
