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
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".heif", ".tiff", ".tif", ".raw", ".cr2", ".nef", ".arw" };
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".mpg", ".mpeg" };

    private readonly BlobService _blobService;
    private readonly AlbumService _albumService;
    private readonly PhotoService _photoService;
    private readonly DestinationSettingsService _destinationSettings;
    private readonly IFolderPickerService _folderPicker;
    private readonly ILogger<MainViewModel> _logger;

    private List<CorePhotoItem> _allPhotos = new();

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "Not connected";

    [ObservableProperty]
    private AlbumModel? _selectedAlbum;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _newAlbumName = string.Empty;

    [ObservableProperty]
    private bool _isCreateMode;

    [ObservableProperty]
    private bool _isNewContainer = true;

    [ObservableProperty]
    private string _selectedExistingContainer = string.Empty;

    [ObservableProperty]
    private bool _isRenameMode;

    [ObservableProperty]
    private AlbumModel? _renamingAlbum;

    [ObservableProperty]
    private string _renameAlbumName = string.Empty;

    [ObservableProperty]
    private bool _isFilterPhoto = true;

    [ObservableProperty]
    private bool _isFilterVideo = true;

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private bool _isYearFilterExpanded;

    [ObservableProperty]
    private bool _isOlderYearsExpanded;

    public ObservableCollection<AlbumModel> Albums { get; } = new();
    public ObservableCollection<CorePhotoItem> Photos { get; } = new();
    public ObservableCollection<CorePhotoItem> SelectedPhotos { get; } = new();
    public ObservableCollection<string> AvailableContainers { get; } = new();
    public ObservableCollection<int> RecentYears { get; } = new();
    public ObservableCollection<int> OlderYears { get; } = new();

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

        SelectedPhotos.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSelection));

        if (!string.IsNullOrEmpty(_destinationSettings.AzureConnectionString))
        {
            _ = ConnectAsync();
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        var cs = _destinationSettings.AzureConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
        {
            ConnectionStatus = "No connection string configured in Settings";
            return;
        }

        IsLoading = true;
        ConnectionStatus = "Connecting...";

        try
        {
            if (_blobService.Connect(cs))
            {
                IsConnected = true;
                ConnectionStatus = "Connected";
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
            var registeredContainers = _destinationSettings.AzureAlbums
                .Select(a => a.ContainerName).ToHashSet();

            var allContainers = await GetAllContainerNamesAsync();
            var albums = new List<AlbumModel>();

            foreach (var containerName in allContainers)
            {
                if (!registeredContainers.Contains(containerName))
                    continue;

                var entry = _destinationSettings.AzureAlbums
                    .First(a => a.ContainerName == containerName);
                var blobCount = await _albumService.GetBlobCountAsync(containerName);

                albums.Add(new AlbumModel
                {
                    Name = entry.DisplayName,
                    Uri = containerName,
                    PhotoCount = blobCount
                });
            }

            Albums.Clear();
            foreach (var album in albums.OrderBy(a => a.Name))
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
        SelectedYear = 0;
        IsOlderYearsExpanded = false;
        if (value != null)
        {
            _ = LoadPhotosAsync(value.Uri);
        }
        else
        {
            _allPhotos.Clear();
            Photos.Clear();
            SelectedPhotos.Clear();
            RecentYears.Clear();
            OlderYears.Clear();
        }
    }

    private async Task LoadPhotosAsync(string containerName)
    {
        IsLoading = true;
        try
        {
            var allPhotos = await _photoService.ListPhotosAsync(containerName);
            _allPhotos = allPhotos.Where(p =>
            {
                var ext = Path.GetExtension(p.BlobName);
                return ImageExtensions.Contains(ext) || VideoExtensions.Contains(ext);
            }).ToList();

            BuildYearFilters();
            ApplyFilters();
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

    private void BuildYearFilters()
    {
        var years = _allPhotos
            .Select(p => p.LastModified?.Year ?? DateTime.MinValue.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToList();

        RecentYears.Clear();
        OlderYears.Clear();

        var now = DateTime.Now.Year;
        foreach (var year in years)
        {
            if (year <= 0) continue;
            if (RecentYears.Count < 5 && year >= now - 4)
                RecentYears.Add(year);
            else
                OlderYears.Add(year);
        }
    }

    [RelayCommand]
    private void ToggleFilterPhoto() { IsFilterPhoto = !IsFilterPhoto; }

    [RelayCommand]
    private void ToggleFilterVideo() { IsFilterVideo = !IsFilterVideo; }

    partial void OnIsFilterPhotoChanged(bool value) => ApplyFilters();
    partial void OnIsFilterVideoChanged(bool value) => ApplyFilters();
    partial void OnSelectedYearChanged(int value) => ApplyFilters();

    private void ApplyFilters()
    {
        var filtered = _allPhotos.AsEnumerable();

        if (IsFilterPhoto && !IsFilterVideo)
            filtered = filtered.Where(p => ImageExtensions.Contains(Path.GetExtension(p.BlobName)));
        else if (!IsFilterPhoto && IsFilterVideo)
            filtered = filtered.Where(p => VideoExtensions.Contains(Path.GetExtension(p.BlobName)));

        if (SelectedYear > 0)
            filtered = filtered.Where(p => (p.LastModified?.Year ?? DateTime.MinValue.Year) == SelectedYear);

        Photos.Clear();
        SelectedPhotos.Clear();
        foreach (var photo in filtered)
            Photos.Add(photo);

        StatusMessage = $"Showing {Photos.Count} of {_allPhotos.Count} files";
    }

    [RelayCommand]
    private void ToggleYearFilter() => IsYearFilterExpanded = !IsYearFilterExpanded;

    [RelayCommand]
    private void ToggleOlderYears() => IsOlderYearsExpanded = !IsOlderYearsExpanded;

    [RelayCommand]
    private void SetYearFilter(int year)
    {
        SelectedYear = SelectedYear == year ? 0 : year;
    }

    [RelayCommand]
    private void ShowCreateAlbum()
    {
        IsCreateMode = true;
        NewAlbumName = string.Empty;
        IsNewContainer = true;
        SelectedExistingContainer = string.Empty;
        _ = LoadAvailableContainersAsync();
    }

    [RelayCommand]
    private void CancelCreateAlbum() => IsCreateMode = false;

    private async Task LoadAvailableContainersAsync()
    {
        AvailableContainers.Clear();
        try
        {
            var allContainers = await GetAllContainerNamesAsync();
            var registered = _destinationSettings.AzureAlbums
                .Select(a => a.ContainerName).ToHashSet();
            foreach (var c in allContainers.Where(c => !registered.Contains(c)).OrderBy(c => c))
                AvailableContainers.Add(c);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load containers");
        }
    }

    [RelayCommand]
    private async Task CreateAlbumAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAlbumName)) return;
        if (!IsConnected) return;

        try
        {
            string containerName;
            if (IsNewContainer)
            {
                containerName = $"photos-{Guid.NewGuid():N}";
                await _albumService.CreateAlbumAsync(containerName);
            }
            else
            {
                containerName = SelectedExistingContainer;
                if (string.IsNullOrEmpty(containerName))
                {
                    StatusMessage = "Select an existing container or choose New";
                    return;
                }
            }

            _destinationSettings.AddAzureAlbum(NewAlbumName, containerName);
            IsCreateMode = false;
            await LoadAlbumsAsync();
            StatusMessage = $"Album '{NewAlbumName}' created";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to create album: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowRenameAlbum(AlbumModel? album)
    {
        if (album == null) return;
        RenamingAlbum = album;
        RenameAlbumName = album.Name;
        IsRenameMode = true;
    }

    [RelayCommand]
    private void CancelRenameAlbum()
    {
        IsRenameMode = false;
        RenamingAlbum = null;
    }

    [RelayCommand]
    private async Task ConfirmRenameAlbumAsync()
    {
        if (RenamingAlbum == null || string.IsNullOrWhiteSpace(RenameAlbumName)) return;
        if (!IsConnected) return;

        try
        {
            var containerName = RenamingAlbum.Uri;
            if (RenameAlbumName != RenamingAlbum.Name)
            {
                await _albumService.RenameAlbumAsync(containerName, RenameAlbumName);
                _destinationSettings.RenameAzureAlbum(containerName, RenameAlbumName);
            }

            IsRenameMode = false;
            RenamingAlbum = null;
            await LoadAlbumsAsync();
            StatusMessage = $"Album renamed to '{RenameAlbumName}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to rename album: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAlbumAsync(AlbumModel? album)
    {
        if (album == null) return;
        if (!IsConnected) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Album",
            $"Delete '{album.Name}' and all its files?\n\nThis cannot be undone.",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _albumService.DeleteAlbumAsync(album.Uri);
            _destinationSettings.RemoveAzureAlbum(album.Uri);
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
    private async Task UploadPhotoAsync()
    {
        if (SelectedAlbum == null || !IsConnected) return;

        try
        {
            var result = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
                        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".m4v" } },
                    { DevicePlatform.macOS, new[] { "public.image", "public.movie" } },
                    { DevicePlatform.iOS, new[] { "public.image", "public.movie" } },
                    { DevicePlatform.Android, new[] { "image/*", "video/*" } }
                }),
                PickerTitle = "Select photos or videos"
            });

            if (result == null || !result.Any()) return;
            await UploadFilesAsync(result.Select(f => f.FullPath).ToList());
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UploadDroppedFilesAsync(List<string> filePaths)
    {
        if (filePaths == null || filePaths.Count == 0) return;
        await UploadFilesAsync(filePaths);
    }

    private async Task UploadFilesAsync(List<string> filePaths)
    {
        if (SelectedAlbum == null || !IsConnected) return;

        IsLoading = true;
        try
        {
            var count = 0;
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath)) continue;
                using var stream = File.OpenRead(filePath);
                var fileName = Path.GetFileName(filePath);
                await _photoService.UploadPhotoAsync(SelectedAlbum.Uri, stream, fileName);
                count++;
            }

            await LoadPhotosAsync(SelectedAlbum.Uri);
            await LoadAlbumsAsync();
            StatusMessage = $"Uploaded {count} file(s)";
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

            StatusMessage = $"Downloaded {count} file(s) to {folder}";
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
            "Delete Files",
            $"Delete {SelectedPhotos.Count} selected file(s)?\n\nThis cannot be undone.",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            var names = SelectedPhotos.Select(p => p.BlobName).ToList();
            await _photoService.DeletePhotosAsync(SelectedAlbum.Uri, names);
            await LoadPhotosAsync(SelectedAlbum.Uri);
            await LoadAlbumsAsync();
            StatusMessage = $"Deleted {names.Count} file(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ShowAlbumActionsAsync(AlbumModel? album)
    {
        if (album == null) return;

        var action = await Application.Current!.MainPage!.DisplayActionSheet(
            album.Name, "Cancel", null, "Rename", "Delete");

        if (action == "Rename")
            ShowRenameAlbum(album);
        else if (action == "Delete")
            await DeleteAlbumAsync(album);
    }

    private async Task<List<string>> GetAllContainerNamesAsync()
    {
        var client = _blobService.GetClient();
        var names = new List<string>();
        await foreach (var container in client.GetBlobContainersAsync())
            names.Add(container.Name);
        return names;
    }

    [RelayCommand]
    private async Task OpenSlideshowAsync()
    {
        if (Photos.Count == 0) return;

        var slideshowPage = new Pages.SlideshowPage(Photos.ToList(), SelectedAlbum?.Name ?? "");
        await Application.Current!.MainPage!.Navigation.PushModalAsync(slideshowPage);
    }
}
