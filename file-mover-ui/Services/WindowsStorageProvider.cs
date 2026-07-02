using FileMover.UI.Models;

namespace FileMover.UI.Services;

public class WindowsStorageProvider : IStorageProviderService
{
    public string ProviderType => "local";

    public Task<List<string>> ListFoldersAsync(string path)
    {
        if (!Directory.Exists(path))
            return Task.FromResult(new List<string>());

        var folders = Directory.EnumerateDirectories(path)
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .Select(f => f!)
            .ToList();

        return Task.FromResult(folders);
    }

    public Task<List<FileItem>> ListFilesAsync(string path)
    {
        if (!Directory.Exists(path))
            return Task.FromResult(new List<FileItem>());

        var files = Directory.EnumerateFiles(path)
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new FileItem
                {
                    Id = info.FullName,
                    Name = info.Name,
                    Path = info.FullName,
                    IsFolder = false,
                    Size = info.Length,
                    Modified = info.LastWriteTimeUtc,
                    MimeType = GetMimeType(info.Extension)
                };
            })
            .ToList();

        return Task.FromResult(files);
    }

    public async Task CreateFileAsync(string path, Stream contentStream)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var fileStream = File.Create(path);
        await contentStream.CopyToAsync(fileStream);
    }

    public async Task UploadFileAsync(string localPath, string remotePath)
    {
        var dir = Path.GetDirectoryName(remotePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var sourceStream = File.OpenRead(localPath);
        await using var destStream = File.Create(remotePath);
        await sourceStream.CopyToAsync(destStream);
    }

    public Task RenameAsync(string oldPath, string newName)
    {
        var dir = Path.GetDirectoryName(oldPath);
        var newPath = Path.Combine(dir ?? string.Empty, newName);

        if (Directory.Exists(oldPath))
            Directory.Move(oldPath, newPath);
        else
            File.Move(oldPath, newPath);

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

    private static string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }
}
