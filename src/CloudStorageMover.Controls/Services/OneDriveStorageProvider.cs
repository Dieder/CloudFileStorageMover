using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Controls.Services;

public class OneDriveStorageProvider : IStorageProviderService
{
    private readonly ILogger<OneDriveStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.OneDrive;
    public string DisplayName => "OneDrive";

    public OneDriveStorageProvider(ILogger<OneDriveStorageProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync()
    {
        var oneDrivePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var possiblePaths = new[]
        {
            Path.Combine(oneDrivePath, "OneDrive"),
            Path.Combine(oneDrivePath, "OneDrive - Personal"),
            oneDrivePath
        };

        var dirs = possiblePaths.Where(Directory.Exists).ToList();
        if (dirs.Count == 0)
            dirs.Add("/");

        return Task.FromResult(dirs);
    }

    public Task<List<FileItem>> ListFoldersAsync(string path)
    {
        _logger.LogWarning("OneDrive provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task<List<FileItem>> ListFilesAsync(string path)
    {
        _logger.LogWarning("OneDrive provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task UploadAsync(string localPath, string remotePath, Stream content)
    {
        _logger.LogWarning("OneDrive upload not yet implemented");
        return Task.CompletedTask;
    }

    public Task DownloadAsync(string remotePath, string localPath)
    {
        _logger.LogWarning("OneDrive download not yet implemented");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path)
    {
        _logger.LogWarning("OneDrive delete not yet implemented");
        return Task.CompletedTask;
    }
}
