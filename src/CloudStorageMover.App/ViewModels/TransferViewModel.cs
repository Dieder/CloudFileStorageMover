using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class TransferViewModel : ObservableObject
{
    private readonly SelectedFilesService _selectedFiles;
    private readonly DestinationSettingsService _destinationSettings;
    private readonly DownloadQueue _downloadQueue;
    private readonly ILogger<TransferViewModel> _logger;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _selectedFilesCount;

    [ObservableProperty]
    private string _selectedFilesSummary = string.Empty;

    [ObservableProperty]
    private string _selectedDestination = string.Empty;

    [ObservableProperty]
    private string _selectedDestinationSummary = string.Empty;

    [ObservableProperty]
    private bool _isPreviewVisible;

    [ObservableProperty]
    private bool _isDestinationVisible = true;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private int _importedCount;

    [ObservableProperty]
    private int _failedCount;

    public bool HasSelectedDestination => !string.IsNullOrEmpty(SelectedDestination);

    public ObservableCollection<TransferPreviewItem> PreviewItems { get; } = new();
    public ObservableCollection<DestinationItem> Destinations { get; } = new();

    public event EventHandler? BackRequested;

    public TransferViewModel(SelectedFilesService selectedFiles,
                              DestinationSettingsService destinationSettings,
                              DownloadQueue downloadQueue,
                              ILogger<TransferViewModel> logger)
    {
        _selectedFiles = selectedFiles;
        _destinationSettings = destinationSettings;
        _downloadQueue = downloadQueue;
        _logger = logger;

        _selectedFiles.SelectionChanged += OnSelectionChanged;
    }

    public void Refresh()
    {
        _logger.LogInformation("TransferViewModel.Refresh: files={Count}, totalSize={Size}",
            _selectedFiles.Count, _selectedFiles.TotalSize);
        UpdateSelectionCount();
        LoadDestinations();
        IsPreviewVisible = false;
        IsDestinationVisible = true;
        StatusMessage = string.Empty;

        if (Destinations.Count == 1)
        {
            SelectDestinationInternal(Destinations[0]);
        }
    }

    private void OnSelectionChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateSelectionCount);
    }

    private void UpdateSelectionCount()
    {
        SelectedFilesCount = _selectedFiles.Count;
        SelectedFilesSummary = _selectedFiles.Count > 0
            ? $"{_selectedFiles.Count} file(s) selected ({_selectedFiles.TotalSize / 1024.0 / 1024.0:F1} MB)"
            : "No files selected";
    }

    private void LoadDestinations()
    {
        Destinations.Clear();

        Destinations.Add(new DestinationItem
        {
            Name = _destinationSettings.DestinationPath,
            DisplayLabel = $"Default: {_destinationSettings.DestinationPath}",
            IsRemovable = false
        });

        foreach (var drive in DestinationSettingsService.GetRemovableDrives())
        {
            Destinations.Add(new DestinationItem
            {
                Name = drive.Name,
                DisplayLabel = $"{drive.DisplayLabel} — {drive.FreeSpaceText}",
                IsRemovable = true
            });
        }

        if (Destinations.Count == 1)
        {
            SelectedDestination = Destinations[0].Name;
            Destinations[0].IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectDestination(DestinationItem destination)
    {
        SelectDestinationInternal(destination);
    }

    private void SelectDestinationInternal(DestinationItem destination)
    {
        SelectedDestination = destination.Name;
        SelectedDestinationSummary = $"Destination: {destination.DisplayLabel}";
        foreach (var d in Destinations)
            d.IsSelected = d == destination;
        OnPropertyChanged(nameof(HasSelectedDestination));
    }

    [RelayCommand]
    private void BackToSource()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ShowPreview()
    {
        if (_selectedFiles.Count == 0 || string.IsNullOrEmpty(SelectedDestination))
        {
            StatusMessage = "Select files and a destination first.";
            return;
        }

        PreviewItems.Clear();

        var grouped = new Dictionary<string, List<SelectedFile>>();

        foreach (var file in _selectedFiles.Files)
        {
            var category = FileClassificationService.Classify(file.FileName);
            var destDir = _destinationSettings.GetTransferDestination(category);
            var relativePath = destDir.Replace(_destinationSettings.DestinationPath, "").TrimStart('\\', '/');
            var groupKey = string.IsNullOrEmpty(relativePath) ? "Other" : relativePath;

            if (!grouped.ContainsKey(groupKey))
                grouped[groupKey] = new List<SelectedFile>();

            grouped[groupKey].Add(file);
        }

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            foreach (var file in group.Value)
            {
                var category = FileClassificationService.Classify(file.FileName);
                var destDir = _destinationSettings.GetTransferDestination(category);

                PreviewItems.Add(new TransferPreviewItem
                {
                    FileName = file.FileName,
                    Size = file.Size,
                    Category = category.ToString(),
                    DestinationFolder = destDir.Replace(_destinationSettings.DestinationPath, "").TrimStart('\\', '/'),
                    Source = _selectedFiles.CurrentProvider?.DisplayName ?? "Unknown"
                });
            }
        }

        IsPreviewVisible = true;
        IsDestinationVisible = false;
        StatusMessage = $"Ready to transfer {PreviewItems.Count} file(s) to {SelectedDestination}";
    }

    [RelayCommand]
    private void BackToDestination()
    {
        IsPreviewVisible = false;
        IsDestinationVisible = true;
    }

    [RelayCommand]
    private async Task ExecuteTransferAsync()
    {
        if (_selectedFiles.Count == 0 || string.IsNullOrEmpty(SelectedDestination))
            return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Transfer",
            $"Transfer {_selectedFiles.Count} file(s) to {SelectedDestination}?\n\n" +
            $"Photos will be organized by date.\n" +
            $"Videos and documents will be in their own folders.",
            "Transfer",
            "Cancel");

        if (!confirm)
            return;

        IsExecuting = true;
        StatusMessage = "Transferring...";

        try
        {
            var imported = 0;
            var failed = 0;

            foreach (var file in _selectedFiles.Files.ToList())
            {
                try
                {
                    var category = FileClassificationService.Classify(file.FileName);
                    var destDir = _destinationSettings.GetTransferDestination(category);
                    Directory.CreateDirectory(destDir);

                    var localPath = Path.Combine(destDir, file.FileName);
                    var counter = 1;
                    while (File.Exists(localPath))
                    {
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        localPath = Path.Combine(destDir, $"{nameWithoutExt} ({counter}){ext}");
                        counter++;
                    }

                    var job = new DownloadJob
                    {
                        FileName = file.FileName,
                        RemotePath = file.RemotePath,
                        LocalPath = localPath,
                        TotalBytes = file.Size,
                        Provider = _selectedFiles.CurrentProvider
                    };

                    _downloadQueue.Enqueue(job);
                    _selectedFiles.RemoveFile(file.FileId);
                    imported++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enqueue {File}", file.FileName);
                    failed++;
                }
            }

            ImportedCount = imported;
            FailedCount = failed;
            StatusMessage = $"Transfer started: {imported} file(s) queued, {failed} failed.";
            IsExecuting = false;

            _downloadQueue.JobCompleted += OnJobCompleted;
            _downloadQueue.JobFailed += OnJobFailed;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Transfer error: {ex.Message}";
            IsExecuting = false;
        }
    }

    private void OnJobCompleted(DownloadJob job)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _logger.LogInformation("Transfer completed: {File}", job.FileName);
        });
    }

    private void OnJobFailed(DownloadJob job)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _logger.LogWarning("Transfer failed: {File} - {Error}", job.FileName, job.ErrorMessage);
        });
    }
}

public class TransferPreviewItem
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DestinationFolder { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public string SizeText => Size > 1048576
        ? $"{Size / 1048576.0:F1} MB"
        : $"{Size / 1024.0:F0} KB";
}

public class DestinationItem
{
    public string Name { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public bool IsRemovable { get; set; }
    public bool IsSelected { get; set; }
}
