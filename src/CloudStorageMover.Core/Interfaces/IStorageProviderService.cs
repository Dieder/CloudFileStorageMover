using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Interfaces;

public interface IStorageProviderService
{
    StorageProviderType ProviderType { get; }
    string DisplayName { get; }

    Task<List<FileItem>> ListFoldersAsync(string path);
    Task<List<FileItem>> ListFilesAsync(string path);
    Task<List<string>> GetCommonDirectoriesAsync();
    Task UploadAsync(string localPath, string remotePath, Stream content);
    Task DownloadAsync(string remotePath, string localPath);
    Task DeleteAsync(string path);
}
