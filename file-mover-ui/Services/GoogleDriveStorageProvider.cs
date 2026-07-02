using FileMover.UI.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace FileMover.UI.Services;

public class GoogleDriveStorageProvider : IStorageProviderService
{
    public string ProviderType => "gcs";

    private DriveService? _driveService;
    private readonly ProviderConfigLoader _configLoader;
    private ProviderConfig? _config;

    public GoogleDriveStorageProvider(ProviderConfigLoader configLoader)
    {
        _configLoader = configLoader;
    }

    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService is not null)
            return _driveService;

        _config = await _configLoader.LoadAsync("google-drive");
        if (_config is null)
            throw new InvalidOperationException("Google Drive provider configuration not found.");

        var secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ClientSecretsPath ?? "config/client_secrets.json");
        UserCredential credential;

        using (var stream = new FileStream(secretsPath, FileMode.Open, FileAccess.Read))
        {
            var secrets = await GoogleClientSecrets.FromStreamAsync(stream);
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets.Secrets,
                _config.Scopes ?? ["https://www.googleapis.com/auth/drive.file"],
                "user",
                CancellationToken.None,
                new FileDataStore("FileMover.GoogleDrive", true));
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _config.ApplicationName ?? "FileMover"
        });

        return _driveService;
    }

    public async Task<List<string>> ListFoldersAsync(string parentId)
    {
        var service = await GetDriveServiceAsync();
        var request = service.Files.List();
        request.Q = $"'{parentId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
        request.Fields = "files(id, name)";

        var result = await request.ExecuteAsync();
        return result.Files.Select(f => f.Name).ToList();
    }

    public async Task<List<FileItem>> ListFilesAsync(string folderId)
    {
        var service = await GetDriveServiceAsync();
        var request = service.Files.List();
        request.Q = $"'{folderId}' in parents and trashed = false";
        request.Fields = "files(id, name, mimeType, size, modifiedTime)";

        var result = await request.ExecuteAsync();
        return result.Files.Select(f => new FileItem
        {
            Id = f.Id,
            Name = f.Name,
            Path = f.Id,
            IsFolder = f.MimeType == "application/vnd.google-apps.folder",
            Size = f.Size ?? 0,
            Modified = f.ModifiedTime ?? DateTime.UtcNow,
            MimeType = f.MimeType
        }).ToList();
    }

    public async Task CreateFileAsync(string path, Stream contentStream)
    {
        var service = await GetDriveServiceAsync();
        var parentId = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "root";
        var fileName = Path.GetFileName(path);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = [parentId]
        };

        var request = service.Files.Create(fileMetadata, contentStream, "application/octet-stream");
        request.Fields = "id";
        await request.UploadAsync();
    }

    public async Task UploadFileAsync(string localPath, string remotePath)
    {
        var service = await GetDriveServiceAsync();
        var parentId = Path.GetDirectoryName(remotePath)?.Replace('\\', '/') ?? "root";
        var fileName = Path.GetFileName(remotePath);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = [parentId]
        };

        await using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
        var request = service.Files.Create(fileMetadata, stream, GetMimeType(localPath));
        request.Fields = "id";
        await request.UploadAsync();
    }

    public async Task RenameAsync(string fileId, string newName)
    {
        var service = await GetDriveServiceAsync();
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = newName
        };
        var request = service.Files.Update(fileMetadata, fileId);
        request.Fields = "id, name";
        await request.ExecuteAsync();
    }

    public async Task DeleteAsync(string fileId)
    {
        var service = await GetDriveServiceAsync();
        await service.Files.Delete(fileId).ExecuteAsync();
    }

    private static string GetMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        return ext switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }
}
