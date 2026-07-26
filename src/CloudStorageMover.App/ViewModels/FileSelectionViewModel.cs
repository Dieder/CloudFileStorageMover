using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class FileSelectionViewModel : ObservableObject
{
    private readonly SelectedFilesService _selectedFiles;
    private readonly DestinationSettingsService _destinationSettings;
    private readonly DownloadQueue _downloadQueue;
    private readonly ILogger<FileSelectionViewModel> _logger;

    private IStorageProviderService? _currentProvider;
    private string _currentPath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _currentFolderName = string.Empty;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private string _selectedSummary = string.Empty;

    public ObservableCollection<SelectableFileItem> Files { get; } = [];

    public event Action? NavigateToTransferRequested;

    public FileSelectionViewModel(SelectedFilesService selectedFiles,
                                   DestinationSettingsService destinationSettings,
                                   DownloadQueue downloadQueue,
                                   ILogger<FileSelectionViewModel> logger)
    {
        _selectedFiles = selectedFiles;
        _destinationSettings = destinationSettings;
        _downloadQueue = downloadQueue;
        _logger = logger;

        _selectedFiles.SelectionChanged += OnSelectionChanged;
    }

    public async Task LoadFolderAsync(IStorageProviderService provider, FileItem folder)
    {
        _currentProvider = provider;
        _currentPath = folder.FullPath;
        CurrentFolderName = folder.Name;
        _selectedFiles.SetProvider(provider);

        IsLoading = true;
        try
        {
            var files = await provider.ListFilesAsync(folder.FullPath);
            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(new SelectableFileItem(file));
            }
            UpdateSummary();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load files for folder {Folder}", folder.FullPath);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleFileSelection(SelectableFileItem item)
    {
        item.IsSelected = !item.IsSelected;

        if (item.IsSelected)
            _selectedFiles.AddFile(item.FileItem, item.FileItem.FullPath);
        else
            _selectedFiles.RemoveFile(item.FileItem.FullPath);

        UpdateSummary();
    }

    [RelayCommand]
    private async Task TransferSelectedAsync()
    {
        if (_currentProvider == null || _selectedFiles.Count == 0)
            return;

        if (!_destinationSettings.HasValidDestination())
        {
            await Shell.Current.DisplayAlert("Invalid Destination",
                "Please set a valid destination folder in Configuration.", "OK");
            return;
        }

        var totalSize = _selectedFiles.TotalSize;
        var freeSpace = _destinationSettings.GetFreeSpace();

        if (totalSize > freeSpace)
        {
            var proceed = await Shell.Current.DisplayAlert("Low Disk Space",
                $"Total file size ({totalSize / 1024.0 / 1024.0:F1} MB) exceeds available disk space " +
                $"({freeSpace / 1024.0 / 1024.0:F1} MB).\n\nContinue anyway?",
                "Yes", "No");
            if (!proceed)
                return;
        }

        var pendingTransfers = new List<string>();

        foreach (var selectedFile in _selectedFiles.Files.ToList())
        {
            var category = FileClassificationService.Classify(selectedFile.FileName);
            var destDir = _destinationSettings.GetTransferDestination(category);
            Directory.CreateDirectory(destDir);

            var localPath = Path.Combine(destDir, selectedFile.FileName);
            var counter = 1;
            while (File.Exists(localPath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(selectedFile.FileName);
                var ext = Path.GetExtension(selectedFile.FileName);
                localPath = Path.Combine(destDir, $"{nameWithoutExt} ({counter}){ext}");
                counter++;
            }

            pendingTransfers.Add(selectedFile.FileName);

            var job = new DownloadJob
            {
                FileName = selectedFile.FileName,
                RemotePath = selectedFile.RemotePath,
                LocalPath = localPath,
                TotalBytes = selectedFile.Size,
                Provider = _currentProvider
            };

            _downloadQueue.Enqueue(job);
            _selectedFiles.RemoveFile(selectedFile.FileId);
        }

        _downloadQueue.JobCompleted += OnTransferJobCompleted;
        _downloadQueue.JobFailed += OnTransferJobFailed;

        UpdateSummary();

        NavigateToTransferRequested?.Invoke();
    }

    private void OnTransferJobCompleted(DownloadJob job)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _logger.LogInformation("Transfer completed: {File}", job.FileName);
            RemoveFileFromList(job.FileName);
        });
    }

    private void OnTransferJobFailed(DownloadJob job)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _logger.LogWarning("Transfer failed: {File} - {Error}", job.FileName, job.ErrorMessage);
        });
    }

    private void RemoveFileFromList(string fileName)
    {
        var item = Files.FirstOrDefault(f =>
            f.FileItem.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (item != null)
        {
            Files.Remove(item);
        }
    }

    private void OnSelectionChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateSummary);
    }

    private void UpdateSummary()
    {
        SelectedCount = _selectedFiles.Count;
        SelectedSummary = _selectedFiles.Count > 0
            ? $"{_selectedFiles.Count} file(s) selected ({_selectedFiles.TotalSize / 1024.0 / 1024.0:F1} MB)"
            : "No files selected";
    }
}
