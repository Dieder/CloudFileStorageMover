using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Services;

public class SelectedFilesService
{
    private readonly List<SelectedFile> _files = new();

    public IReadOnlyList<SelectedFile> Files => _files.AsReadOnly();

    public int Count => _files.Count;

    public long TotalSize => _files.Sum(f => f.Size);

    public IStorageProviderService? CurrentProvider { get; private set; }
    public StorageProviderType? CurrentProviderType { get; private set; }

    public event Action? SelectionChanged;
    public event Action? ProviderChanged;

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
        _files.Clear();
        SelectionChanged?.Invoke();
    }

    public (string LocalPath, string RemotePath, IStorageProviderService Provider) ResolveDownloadTarget(SelectedFile file)
    {
        var destDir = GetDestinationDirectory();
        var localPath = Path.Combine(destDir, file.FileName);

        var counter = 1;
        while (File.Exists(localPath))
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            localPath = Path.Combine(destDir, $"{nameWithoutExt} ({counter}){ext}");
            counter++;
        }

        return (localPath, file.RemotePath, CurrentProvider!);
    }

    public string GetDestinationDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
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
