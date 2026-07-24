using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Interfaces;

public interface IStorageProviderService
{
    StorageProviderType ProviderType { get; }
    string DisplayName { get; }

    Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default);
    Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default);
    Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default);
    Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default);
    Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default);
    Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
}
