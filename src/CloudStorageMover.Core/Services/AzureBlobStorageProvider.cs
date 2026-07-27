using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class AzureBlobStorageProvider : IStorageProviderService
{
    private readonly BlobService _blobService;
    private readonly DestinationSettingsService _settings;
    private readonly ILogger<AzureBlobStorageProvider>? _logger;

    public StorageProviderType ProviderType => StorageProviderType.AzureBlob;
    public string DisplayName => "Azure Blob Storage";

    public AzureBlobStorageProvider(BlobService blobService, DestinationSettingsService settings, ILogger<AzureBlobStorageProvider>? logger = null)
    {
        _blobService = blobService;
        _settings = settings;
        _logger = logger;
    }

    public Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        var dirs = _settings.AzureAlbums
            .Where(a => !string.IsNullOrEmpty(a.ContainerName))
            .Select(a => $"{a.ContainerName}|{a.DisplayName}")
            .ToList();
        return Task.FromResult(dirs);
    }

    public Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage. Configure the connection string first.");

        var client = _blobService.GetClient();
        var (containerName, prefix) = SplitPath(path);

        var containerClient = client.GetBlobContainerClient(containerName);
        var folders = new List<FileItem>();
        var seenDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var effectivePrefix = string.IsNullOrEmpty(prefix) ? "" : prefix.TrimEnd('/') + "/";

        foreach (var blobItem in containerClient.GetBlobs(prefix: effectivePrefix))
        {
            var name = blobItem.Name;
            if (name.Length <= effectivePrefix.Length)
                continue;

            var relativePath = name[effectivePrefix.Length..];
            var slashIndex = relativePath.IndexOf('/');
            if (slashIndex > 0)
            {
                var dirName = relativePath[..slashIndex];
                var fullPath = $"{containerName}/{effectivePrefix}{dirName}";
                if (seenDirs.Add(fullPath))
                {
                    folders.Add(new FileItem
                    {
                        Name = dirName,
                        FullPath = fullPath,
                        IsDirectory = true,
                        ProviderType = ProviderType
                    });
                }
            }
        }

        return Task.FromResult(folders);
    }

    public Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage. Configure the connection string first.");

        var client = _blobService.GetClient();
        var (containerName, prefix) = SplitPath(path);

        var containerClient = client.GetBlobContainerClient(containerName);
        var files = new List<FileItem>();

        var effectivePrefix = string.IsNullOrEmpty(prefix) ? "" : prefix.TrimEnd('/') + "/";

        foreach (var blobItem in containerClient.GetBlobs(prefix: effectivePrefix))
        {
            var name = blobItem.Name;
            if (name.Length <= effectivePrefix.Length)
                continue;

            var relativePath = name[effectivePrefix.Length..];
            if (!relativePath.Contains('/'))
            {
                files.Add(new FileItem
                {
                    Name = relativePath,
                    FullPath = $"{containerName}/{name}",
                    Size = blobItem.Properties.ContentLength ?? 0,
                    ModifiedDate = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                    IsDirectory = false,
                    ProviderType = ProviderType
                });
            }
        }

        return Task.FromResult(files);
    }

    public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage.");

        var (containerName, blobName) = ParseRemotePath(remotePath);
        var client = _blobService.GetClient();
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var download = await blobClient.DownloadAsync(ct);
        return download.Value.Content;
    }

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage.");

        var (containerName, blobName) = ParseRemotePath(remotePath);
        var client = _blobService.GetClient();
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (progress != null && content.CanSeek)
        {
            var totalBytes = content.Length;
            var buffer = new byte[81920];
            long bytesUploaded = 0;
            int bytesRead;
            while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                var stream = new MemoryStream(buffer, 0, bytesRead);
                await blobClient.UploadAsync(stream, overwrite: true, ct);
                bytesUploaded += bytesRead;
                progress.Report(bytesUploaded * 1.0 / totalBytes);
            }
        }
        else
        {
            await blobClient.UploadAsync(content, overwrite: true, ct);
        }
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage.");

        var (containerName, blobName) = ParseRemotePath(remotePath);
        var client = _blobService.GetClient();
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var download = await blobClient.DownloadAsync(ct);

        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        if (progress != null)
        {
            var contentStream = download.Value.Content;
            var totalBytes = download.Value.ContentLength;
            var buffer = new byte[81920];
            long bytesDownloaded = 0;
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                bytesDownloaded += bytesRead;
                if (totalBytes > 0)
                    progress.Report(bytesDownloaded * 1.0 / totalBytes);
            }
        }
        else
        {
            await download.Value.Content.CopyToAsync(fileStream, ct);
        }
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (!_blobService.IsConnected)
            throw new InvalidOperationException("Not connected to Azure Blob Storage.");

        var (containerName, blobName) = ParseRemotePath(path);
        var client = _blobService.GetClient();
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    private static (string ContainerName, string BlobName) ParseRemotePath(string remotePath)
    {
        var slashIndex = remotePath.IndexOf('/');
        if (slashIndex < 0)
            throw new ArgumentException($"Invalid remote path format: {remotePath}. Expected 'containerName/blobName'.");

        return (remotePath[..slashIndex], remotePath[(slashIndex + 1)..]);
    }

    private static (string ContainerName, string? Prefix) SplitPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return (string.Empty, null);

        var slashIndex = path.IndexOf('/');
        if (slashIndex < 0)
            return (path, null);

        return (path[..slashIndex], path[(slashIndex + 1)..]);
    }
}
