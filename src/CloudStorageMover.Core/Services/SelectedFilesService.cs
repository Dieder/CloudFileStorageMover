using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class SelectedFilesService
{
    private readonly DestinationSettingsService _destinationSettings;
    private readonly List<SelectedFile> _files = new();
    private readonly ILogger<SelectedFilesService>? _logger;

    public IReadOnlyList<SelectedFile> Files => _files.AsReadOnly();

    public int Count => _files.Count;

    public long TotalSize => _files.Sum(f => f.Size);

    public IStorageProviderService? CurrentProvider { get; private set; }
    public StorageProviderType? CurrentProviderType { get; private set; }

    public event Action? SelectionChanged;
    public event Action? ProviderChanged;

    public SelectedFilesService(DestinationSettingsService destinationSettings, ILogger<SelectedFilesService>? logger = null)
    {
        _destinationSettings = destinationSettings;
        _logger = logger;
    }

    public void SetProvider(IStorageProviderService provider)
    {
        if (CurrentProvider != null && CurrentProvider.ProviderType != provider.ProviderType && _files.Count > 0)
        {
            Clear();
        }

        CurrentProvider = provider;
        CurrentProviderType = provider.ProviderType;
        ProviderChanged?.Invoke();
    }

    public void AddFile(FileItem item, string remotePath)
    {
        if (_files.Any(f => f.FileId == item.FullPath))
            return;

        _logger?.LogInformation("AddFile: {Name} (total={Count})", item.Name, _files.Count + 1);
        _files.Add(new SelectedFile
        {
            FileId = item.FullPath,
            FileName = item.Name,
            Size = item.Size,
            RemotePath = remotePath,
            ProviderType = item.ProviderType
        });
        SelectionChanged?.Invoke();
    }

    public void AddRange(IEnumerable<(FileItem Item, string RemotePath)> items)
    {
        foreach (var (item, remotePath) in items)
        {
            if (!_files.Any(f => f.FileId == item.FullPath))
            {
                _files.Add(new SelectedFile
                {
                    FileId = item.FullPath,
                    FileName = item.Name,
                    Size = item.Size,
                    RemotePath = remotePath,
                    ProviderType = item.ProviderType
                });
            }
        }
        SelectionChanged?.Invoke();
    }

    public void RemoveFile(string fileId)
    {
        _files.RemoveAll(f => f.FileId == fileId);
        SelectionChanged?.Invoke();
    }

    public void Clear()
    {
        _logger?.LogWarning("Clear called! Count={Count}. Stack trace: {Trace}",
            _files.Count, Environment.StackTrace);
        _files.Clear();
        SelectionChanged?.Invoke();
    }

    public string GetDestinationDirectory()
    {
        return _destinationSettings.DestinationPath;
    }
}

public class SelectedFile
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string RemotePath { get; set; } = string.Empty;
    public StorageProviderType ProviderType { get; set; }
}
