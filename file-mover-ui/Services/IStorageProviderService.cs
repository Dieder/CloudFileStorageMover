using FileMover.UI.Models;

namespace FileMover.UI.Services;

public interface IStorageProviderService
{
    string ProviderType { get; }

    Task<List<string>> ListFoldersAsync(string path);

    Task<List<FileItem>> ListFilesAsync(string path);

    Task CreateFileAsync(string path, Stream contentStream);

    Task UploadFileAsync(string localPath, string remotePath);

    Task RenameAsync(string oldPath, string newName);

    Task DeleteAsync(string path);
}
