using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class ConfigurationViewModel : ObservableObject
{
    private readonly DestinationSettingsService _destinationService;
    private readonly IFolderPickerService _folderPicker;
    private readonly ILogger<ConfigurationViewModel> _logger;

    [ObservableProperty]
    private string _destinationPath = string.Empty;

    [ObservableProperty]
    private bool _isDestinationValid;

    [ObservableProperty]
    private string _googlePhotosDownloadPath = string.Empty;

    [ObservableProperty]
    private bool _isGooglePhotosDownloadValid;

    public ConfigurationViewModel(DestinationSettingsService destinationService,
                                   IFolderPickerService folderPicker,
                                   ILogger<ConfigurationViewModel> logger)
    {
        _destinationService = destinationService;
        _folderPicker = folderPicker;
        _logger = logger;
        _destinationPath = _destinationService.DestinationPath;
        _isDestinationValid = _destinationService.HasValidDestination();
        _googlePhotosDownloadPath = _destinationService.GooglePhotosDownloadPath;
        _isGooglePhotosDownloadValid = _destinationService.HasValidGooglePhotosDestination();
    }

    partial void OnDestinationPathChanged(string value)
    {
        _destinationService.SetDestinationPath(value);
        IsDestinationValid = _destinationService.HasValidDestination();
    }

    partial void OnGooglePhotosDownloadPathChanged(string value)
    {
        _destinationService.SetGooglePhotosDownloadPath(value);
        IsGooglePhotosDownloadValid = _destinationService.HasValidGooglePhotosDestination();
    }

    [RelayCommand]
    private async Task BrowseDestinationAsync()
    {
        try
        {
            var path = await _folderPicker.PickFolderAsync();
            if (!string.IsNullOrEmpty(path))
            {
                DestinationPath = path;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Folder picker cancelled or failed");
        }
    }

    [RelayCommand]
    private async Task BrowseGooglePhotosDownloadAsync()
    {
        try
        {
            var path = await _folderPicker.PickFolderAsync();
            if (!string.IsNullOrEmpty(path))
            {
                GooglePhotosDownloadPath = path;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Folder picker cancelled or failed");
        }
    }

    [RelayCommand]
    private void ResetToDesktop()
    {
        DestinationPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    [RelayCommand]
    private void ResetGooglePhotosToDesktop()
    {
        GooglePhotosDownloadPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Google Photos");
    }
}
