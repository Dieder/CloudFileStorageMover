using System;
using System.Threading;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Controls.Services;

public class MetaStorageProvider : IStorageProviderService
{
    private readonly ILogger<MetaStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.Meta;
    public string DisplayName => "Meta";

    public MetaStorageProvider(ILogger<MetaStorageProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new List<string> { "/Meta Storage" });
    }

    public Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("Meta provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("Meta provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        _logger.LogWarning("Meta upload not yet implemented");
        return Task.CompletedTask;
    }

    public Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        _logger.LogWarning("Meta download not yet implemented");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("Meta delete not yet implemented");
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
