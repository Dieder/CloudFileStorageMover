using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;
using CorePhotoItem = CloudStorageMover.Core.Models.PhotoItem;

namespace CloudStorageMover.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly BlobService _blobService;
    private readonly AlbumService _albumService;
    private readonly PhotoService _photoService;
    private readonly DestinationSettingsService _destinationSettings;
    private readonly IFolderPickerService _folderPicker;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "Not connected";

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private bool _isConnectionStringVisible;

    [ObservableProperty]
    private AlbumModel? _selectedAlbum;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isMultiSelectMode;

    [ObservableProperty]
    private string _newAlbumName = string.Empty;

    [ObservableProperty]
    private bool _isRenameMode;

    [ObservableProperty]
    private string _renameAlbumName = string.Empty;

    public ObservableCollection<AlbumModel> Albums { get; } = new();
    public ObservableCollection<CorePhotoItem> Photos { get; } = new();
    public ObservableCollection<CorePhotoItem> SelectedPhotos { get; } = new();

    public bool HasSelection => SelectedPhotos.Count > 0;
    public bool HasAlbum => SelectedAlbum != null;

    public MainViewModel(
        BlobService blobService,
        AlbumService albumService,
        PhotoService photoService,
        DestinationSettingsService destinationSettings,
        IFolderPickerService folderPicker,
        ILogger<MainViewModel> logger)
    {
        _blobService = blobService;
        _albumService = albumService;
        _photoService = photoService;
        _destinationSettings = destinationSettings;
        _folderPicker = folderPicker;
        _logger = logger;

        ConnectionString = _destinationSettings.AzureConnectionString;
        if (!string.IsNullOrEmpty(ConnectionString))
        {
            _ = ConnectAsync();
        }

        SelectedPhotos.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSelection));
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            ConnectionStatus = "No connection string configured";
            return;
        }

        IsLoading = true;
        ConnectionStatus = "Connecting...";

        try
        {
            if (_blobService.Connect(ConnectionString))
            {
                IsConnected = true;
                ConnectionStatus = "Connected";
                _destinationSettings.SetAzureConnectionString(ConnectionString);
                await LoadAlbumsAsync();
            }
            else
            {
                ConnectionStatus = "Connection failed";
            }
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Error: {ex.Message}";
            _logger.LogError(ex, "Connection failed");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadAlbumsAsync()
    {
        if (!IsConnected) return;

        IsLoading = true;
        try
        {
            var albums = await _albumService.ListAlbumsAsync();
            Albums.Clear();
            foreach (var album in albums)
                Albums.Add(album);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load albums: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedAlbumChanged(AlbumModel? value)
    {
        OnPropertyChanged(nameof(HasAlbum));
        if (value != null)
        {
            _ = LoadPhotosAsync(value.Name);
        }
        else
        {
            Photos.Clear();
            SelectedPhotos.Clear();
        }
    }

    private async Task LoadPhotosAsync(string albumName)
    {
        IsLoading = true;
        try
        {
            var photos = await _photoService.ListPhotosAsync(albumName);
            Photos.Clear();
            SelectedPhotos.Clear();
            foreach (var photo in photos)
                Photos.Add(photo);
            StatusMessage = $"Loaded {photos.Count} photos from {albumName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load photos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateAlbumAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAlbumName)) return;
        if (!IsConnected) return;

        try
        {
            var album = await _albumService.CreateAlbumAsync(NewAlbumName);
            Albums.Add(album);
            NewAlbumName = string.Empty;
            StatusMessage = $"Album '{album.Name}' created";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to create album: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAlbumAsync(AlbumModel? album)
    {
        if (album == null) return;
        if (!IsConnected) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Album",
            $"Delete '{album.Name}' and all its photos?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _albumService.DeleteAlbumAsync(album.Name);
            Albums.Remove(album);
            if (SelectedAlbum == album)
                SelectedAlbum = null;
            StatusMessage = $"Album '{album.Name}' deleted";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete album: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RenameAlbumAsync(AlbumModel? album)
    {
        if (album == null || string.IsNullOrWhiteSpace(RenameAlbumName)) return;
        if (!IsConnected) return;

        try
        {
            await _albumService.RenameAlbumAsync(album.Name, RenameAlbumName);
            await LoadAlbumsAsync();
            RenameAlbumName = string.Empty;
            IsRenameMode = false;
            StatusMessage = "Album renamed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to rename album: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UploadPhotoAsync()
    {
        if (SelectedAlbum == null || !IsConnected) return;

        try
        {
            var result = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" } },
                    { DevicePlatform.macOS, new[] { "public.image" } },
                    { DevicePlatform.iOS, new[] { "public.image" } },
                    { DevicePlatform.Android, new[] { "image/*" } }
                }),
                PickerTitle = "Select photos to upload"
            });

            if (result == null || !result.Any()) return;

            IsLoading = true;
            var count = 0;
            foreach (var file in result)
            {
                using var stream = await file.OpenReadAsync();
                var fileName = Path.GetFileName(file.FullPath);
                await _photoService.UploadPhotoAsync(SelectedAlbum.Name, stream, fileName);
                count++;
            }

            await LoadPhotosAsync(SelectedAlbum.Name);
            StatusMessage = $"Uploaded {count} photo(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadSelectedAsync()
    {
        if (SelectedPhotos.Count == 0 || !IsConnected) return;

        try
        {
            var folder = await _folderPicker.PickFolderAsync();
            if (string.IsNullOrEmpty(folder)) return;

            IsLoading = true;
            var count = 0;
            foreach (var photo in SelectedPhotos.ToList())
            {
                using var stream = await _photoService.DownloadPhotoAsync(photo.AlbumName, photo.BlobName);
                var filePath = Path.Combine(folder, photo.BlobName);
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);
                count++;
            }

            StatusMessage = $"Downloaded {count} photo(s) to {folder}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedPhotos.Count == 0 || SelectedAlbum == null || !IsConnected) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Photos",
            $"Delete {SelectedPhotos.Count} selected photo(s)?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            var names = SelectedPhotos.Select(p => p.BlobName).ToList();
            await _photoService.DeletePhotosAsync(SelectedAlbum.Name, names);
            await LoadPhotosAsync(SelectedAlbum.Name);
            StatusMessage = $"Deleted {names.Count} photo(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleMultiSelect()
    {
        IsMultiSelectMode = !IsMultiSelectMode;
        if (!IsMultiSelectMode)
            SelectedPhotos.Clear();
        OnPropertyChanged(nameof(HasSelection));
    }

    [RelayCommand]
    private void ToggleConnectionStringVisibility()
    {
        IsConnectionStringVisible = !IsConnectionStringVisible;
    }

    [RelayCommand]
    private async Task ShowAlbumActionsAsync(AlbumModel? album)
    {
        if (album == null) return;

        var action = await Application.Current!.MainPage!.DisplayActionSheet(
            $"Album: {album.Name}", "Cancel", null, "Rename", "Delete");

        if (action == "Rename")
        {
            IsRenameMode = true;
            RenameAlbumName = album.Name;
        }
        else if (action == "Delete")
        {
            await DeleteAlbumAsync(album);
        }
    }
}
