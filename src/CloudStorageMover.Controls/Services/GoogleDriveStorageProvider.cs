using System;
using System.Threading;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Controls.Services;

public class GoogleDriveStorageProvider : IStorageProviderService
{
    private readonly ILogger<GoogleDriveStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.GoogleDrive;
    public string DisplayName => "Google Drive";

    public GoogleDriveStorageProvider(ILogger<GoogleDriveStorageProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new List<string>
        {
            "/My Drive",
            "/Shared drives",
            "/Computers"
        });
    }

    public Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("GoogleDrive provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("GoogleDrive provider not yet implemented");
        return Task.FromResult(new List<FileItem>());
    }

    public Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        _logger.LogWarning("GoogleDrive upload not yet implemented");
        return Task.CompletedTask;
    }

    public Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        _logger.LogWarning("GoogleDrive download not yet implemented");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        _logger.LogWarning("GoogleDrive delete not yet implemented");
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
