using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class CameraImportViewModel : ObservableObject
{
    private readonly DestinationSettingsService _destinationSettings;
    private readonly ILogger<CameraImportViewModel> _logger;

    [ObservableProperty]
    private bool _isWindowsOnly;

    [ObservableProperty]
    private bool _isNotWindowsOnly;

    [ObservableProperty]
    private bool _isDeviceListVisible = true;

    [ObservableProperty]
    private bool _isPhotoListVisible;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _deleteAfterImport;

    [ObservableProperty]
    private int _photoCount;

    [ObservableProperty]
    private int _toImportCount;

    [ObservableProperty]
    private int _alreadyExistsCount;

    [ObservableProperty]
    private bool _isImportPreviewVisible;

    [ObservableProperty]
    private bool _isConfirmingImport;

    public ObservableCollection<CameraDeviceItem> Devices { get; } = new();
    public ObservableCollection<CameraPhotoItem> Photos { get; } = new();

    public CameraImportViewModel(DestinationSettingsService destinationSettings,
                                  ILogger<CameraImportViewModel> logger)
    {
        _destinationSettings = destinationSettings;
        _logger = logger;
        DeleteAfterImport = true;
    }

    partial void OnDeleteAfterImportChanged(bool value)
    {
        _logger.LogInformation("Delete after import changed to {Value}", value);
    }

    public void ShowImportPreview(int totalFound, int toImport, int alreadyExists)
    {
        PhotoCount = totalFound;
        ToImportCount = toImport;
        AlreadyExistsCount = alreadyExists;
        IsImportPreviewVisible = true;
        IsConfirmingImport = false;
    }

    public void StartConfirmingImport()
    {
        IsConfirmingImport = true;
        IsLoading = true;
        StatusMessage = "Importing...";
    }

    public void FinishImport(int imported, int skipped, int failed)
    {
        IsLoading = false;
        IsConfirmingImport = false;
        StatusMessage = $"Import complete: {imported} imported, {skipped} skipped (duplicates), {failed} failed.";
    }
}

public class CameraDeviceItem
{
    public string DeviceId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
}

public class CameraPhotoItem
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime DateTaken { get; set; }
    public string SizeText => Size > 1048576
        ? $"{Size / 1048576.0:F1} MB"
        : $"{Size / 1024.0:F0} KB";
    public string DateText => DateTaken.ToString("yyyy-MM-dd HH:mm");
}
