using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Services;

public class DesktopStorageProvider : IStorageProviderService
{
    private readonly string _baseDirectory;

    public StorageProviderType ProviderType => StorageProviderType.Desktop;
    public string DisplayName => "Desktop";

    public DesktopStorageProvider()
    {
        _baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_baseDirectory, path);
        if (!Directory.Exists(fullPath))
            return Task.FromResult(new List<FileItem>());

        var folders = new List<FileItem>();
        try
        {
            foreach (var dir in Directory.GetDirectories(fullPath))
            {
                var folderInfo = new DirectoryInfo(dir);
                folders.Add(new FileItem
                {
                    Name = folderInfo.Name,
                    FullPath = dir,
                    IsDirectory = true,
                    ProviderType = ProviderType
                });
            }
        }
        catch
        {
        }

        return Task.FromResult(folders);
    }

    public Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_baseDirectory, path);
        if (!Directory.Exists(fullPath))
            return Task.FromResult(new List<FileItem>());

        var files = new List<FileItem>();
        try
        {
            foreach (var file in Directory.GetFiles(fullPath))
            {
                var fileInfo = new FileInfo(file);
                files.Add(new FileItem
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Size = fileInfo.Length,
                    ModifiedDate = fileInfo.LastWriteTime,
                    IsDirectory = false,
                    ProviderType = ProviderType
                });
            }
        }
        catch
        {
        }

        return Task.FromResult(files);
    }

    public Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        var commonDirs = new List<string> { _baseDirectory };

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!string.IsNullOrEmpty(desktop) && Directory.Exists(desktop))
            commonDirs.Add(desktop);

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrEmpty(documents) && Directory.Exists(documents))
            commonDirs.Add(documents);

        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        if (!string.IsNullOrEmpty(pictures) && Directory.Exists(pictures))
            commonDirs.Add(pictures);

        var videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        if (!string.IsNullOrEmpty(videos) && Directory.Exists(videos))
            commonDirs.Add(videos);

        return Task.FromResult(commonDirs);
    }

    public Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(remotePath) ? remotePath : Path.Combine(_baseDirectory, remotePath);
        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true));
    }

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(remotePath) ? remotePath : Path.Combine(_baseDirectory, remotePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        if (progress != null && content.CanSeek)
        {
            var totalBytes = content.Length;
            var buffer = new byte[81920];
            long bytesUploaded = 0;
            int bytesRead;
            while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                bytesUploaded += bytesRead;
                progress.Report(bytesUploaded * 1.0 / totalBytes);
            }
        }
        else
        {
            await content.CopyToAsync(fileStream, ct);
        }
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(remotePath) ? remotePath : Path.Combine(_baseDirectory, remotePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found on desktop", fullPath);

        if (progress != null)
        {
            var longProgress = new Progress<long>(bytesCopied =>
            {
                var total = new FileInfo(fullPath).Length;
                progress.Report(total > 0 ? bytesCopied * 1.0 / total : 0);
            });
            await AsyncFileOperations.CopyAsync(fullPath, localPath, longProgress, overwrite: true, cancellationToken: ct);
        }
        else
        {
            await AsyncFileOperations.CopyAsync(fullPath, localPath, overwrite: true, cancellationToken: ct);
        }
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_baseDirectory, path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        else
        {
            throw new FileNotFoundException("File or directory not found", fullPath);
        }

        return Task.CompletedTask;
    }
}