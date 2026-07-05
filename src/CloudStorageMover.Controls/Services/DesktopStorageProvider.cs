using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Controls.Services;

public class DesktopStorageProvider : IStorageProviderService
{
    private readonly ILogger<DesktopStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.Desktop;
    public string DisplayName => "Desktop";

    public DesktopStorageProvider(ILogger<DesktopStorageProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync()
    {
        var dirs = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
        };

        return Task.FromResult(dirs.Where(Directory.Exists).Distinct().ToList());
    }

    public Task<List<FileItem>> ListFoldersAsync(string path)
    {
        try
        {
            var dirs = Directory.EnumerateDirectories(path)
                .Select(d => new FileItem
                {
                    Name = Path.GetFileName(d),
                    FullPath = d,
                    IsDirectory = true,
                    ModifiedDate = Directory.GetLastWriteTime(d),
                    ProviderType = StorageProviderType.Desktop
                })
                .OrderBy(f => f.Name)
                .ToList();

            return Task.FromResult(dirs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list folders in {Path}", path);
            return Task.FromResult(new List<FileItem>());
        }
    }

    public Task<List<FileItem>> ListFilesAsync(string path)
    {
        try
        {
            var files = Directory.EnumerateFiles(path)
                .Select(f => new FileItem
                {
                    Name = Path.GetFileName(f),
                    FullPath = f,
                    Size = new FileInfo(f).Length,
                    IsDirectory = false,
                    ModifiedDate = File.GetLastWriteTime(f),
                    ProviderType = StorageProviderType.Desktop
                })
                .OrderBy(f => f.Name)
                .ToList();

            return Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files in {Path}", path);
            return Task.FromResult(new List<FileItem>());
        }
    }

    public Task UploadAsync(string localPath, string remotePath, Stream content)
    {
        var dir = Path.GetDirectoryName(remotePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var fs = new FileStream(remotePath, FileMode.Create, FileAccess.Write);
        content.CopyTo(fs);
        return Task.CompletedTask;
    }

    public Task DownloadAsync(string remotePath, string localPath)
    {
        var dir = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.Copy(remotePath, localPath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
        else if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
