using System;
using System.Threading;
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

    public Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
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

    public Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
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

    public Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
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

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(remotePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var fs = new FileStream(remotePath, FileMode.Create, FileAccess.Write);

        if (progress != null && content.CanSeek)
        {
            var buffer = new byte[81920];
            long totalBytes = content.Length;
            long bytesWritten = 0;
            int bytesRead;
            while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead, ct);
                bytesWritten += bytesRead;
                progress.Report((double)bytesWritten / totalBytes);
            }
        }
        else
        {
            await content.CopyToAsync(fs, 81920, ct);
        }
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var source = new FileStream(remotePath, FileMode.Open, FileAccess.Read);
        using var dest = new FileStream(localPath, FileMode.Create, FileAccess.Write);

        if (progress != null && source.CanSeek)
        {
            var buffer = new byte[81920];
            long totalBytes = source.Length;
            long bytesRead = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await dest.WriteAsync(buffer, 0, read, ct);
                bytesRead += read;
                progress.Report((double)bytesRead / totalBytes);
            }
        }
        else
        {
            await source.CopyToAsync(dest, 81920, ct);
        }
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
        else if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        return Task.FromResult((Stream)File.OpenRead(remotePath));
    }
}
