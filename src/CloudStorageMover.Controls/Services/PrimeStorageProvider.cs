using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Controls.Services;

public class PrimeStorageProvider : IStorageProviderService
{
    private readonly ILogger<PrimeStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.Prime;
    public string DisplayName => "Prime";

    public PrimeStorageProvider(ILogger<PrimeStorageProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync()
    {
        return Task.FromResult(new List<string> { "/Prime Storage" });
    }

    public Task<List<FileItem>> ListFoldersAsync(string path)
    {
        _logger.LogWarning("Prime provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task<List<FileItem>> ListFilesAsync(string path)
    {
        _logger.LogWarning("Prime provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task UploadAsync(string localPath, string remotePath, Stream content)
    {
        _logger.LogWarning("Prime upload not yet implemented");
        return Task.CompletedTask;
    }

    public Task DownloadAsync(string remotePath, string localPath)
    {
        _logger.LogWarning("Prime download not yet implemented");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path)
    {
        _logger.LogWarning("Prime delete not yet implemented");
        return Task.CompletedTask;
    }
}
