using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace CloudStorageMover.Core.Services
{
    public class OneDriveAccessTokenProvider : IAccessTokenProvider
    {
        private readonly OneDriveSignInService _authService;

        public AllowedHostsValidator AllowedHostsValidator { get; } = new()
        {
            AllowedHosts = new HashSet<string> { "graph.microsoft.com" }
        };

        public OneDriveAccessTokenProvider(OneDriveSignInService authService)
        {
            _authService = authService;
        }

        public async Task<string> GetAuthorizationTokenAsync(
            Uri uri,
            Dictionary<string, object>? additionalProperties = null,
            CancellationToken cancellationToken = default)
        {
            return await _authService.GetAccessTokenAsync();
        }
    }

    public class OneDriveStorageProvider : IStorageProviderService
    {
        private readonly OneDriveSignInService _authService;
        private GraphServiceClient? _graphClient;
        private string? _driveId;

        public StorageProviderType ProviderType => StorageProviderType.OneDrive;
        public string DisplayName => "OneDrive";

        public OneDriveStorageProvider(OneDriveSignInService authService)
        {
            _authService = authService;
        }

        private GraphServiceClient GetClient()
        {
            if (_graphClient == null)
            {
                var authProvider = new BaseBearerTokenAuthenticationProvider(
                    new OneDriveAccessTokenProvider(_authService));
                _graphClient = new GraphServiceClient(authProvider);
            }
            return _graphClient;
        }

        private async Task<string> GetDriveIdAsync()
        {
            if (_driveId == null)
            {
                var drive = await GetClient().Me.Drive.GetAsync();
                _driveId = drive?.Id ?? throw new InvalidOperationException("Could not retrieve OneDrive ID.");
            }
            return _driveId;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                return string.Empty;
            return path.TrimStart('/');
        }

        public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to list folders.");

            var client = GetClient();
            var normalizedPath = NormalizePath(path);
            var items = await GetChildrenAsync(client, normalizedPath);

            return items
                .Where(i => i.Folder != null)
                .Select(i => new FileItem
                {
                    Name = i.Name,
                    FullPath = i.Id,
                    IsDirectory = true,
                    ProviderType = ProviderType
                })
                .ToList();
        }

        public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to list files.");

            var client = GetClient();
            var normalizedPath = NormalizePath(path);
            var items = await GetChildrenAsync(client, normalizedPath);

            return items
                .Where(i => i.File != null)
                .Select(i => new FileItem
                {
                    Name = i.Name,
                    FullPath = i.Id,
                    Size = i.Size ?? 0,
                    ModifiedDate = i.LastModifiedDateTime?.DateTime ?? DateTime.MinValue,
                    IsDirectory = false,
                    ProviderType = ProviderType
                })
                .ToList();
        }

        private async Task<List<DriveItem>> GetChildrenAsync(GraphServiceClient client, string path)
        {
            var driveId = await GetDriveIdAsync();

            if (string.IsNullOrEmpty(path))
            {
                var rootChildren = await client.Drives[driveId].Items["root"].Children.GetAsync();
                return rootChildren?.Value ?? new List<DriveItem>();
            }

            var children = await client.Drives[driveId].Items["root"].ItemWithPath(path).Children.GetAsync();
            return children?.Value ?? new List<DriveItem>();
        }

        public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                return new List<string>();

            var client = GetClient();
            var driveId = await GetDriveIdAsync();
            var root = await client.Drives[driveId].Items["root"].GetAsync();
            var rootName = root?.Name ?? "My OneDrive";

            return new List<string>
            {
                rootName
            };
        }

        public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to open a file stream.");

            var client = GetClient();
            var driveId = await GetDriveIdAsync();
            var normalizedPath = NormalizePath(remotePath);

            var stream = await client.Drives[driveId].Items["root"].ItemWithPath(normalizedPath).Content.GetAsync(cancellationToken: ct);
            return stream ?? throw new InvalidOperationException("File not found or could not be downloaded.");
        }

        public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to upload files.");

            var client = GetClient();
            var driveId = await GetDriveIdAsync();
            var normalizedPath = NormalizePath(remotePath);

            if (progress != null && content.CanSeek)
            {
                var totalBytes = content.Length;
                var buffer = new byte[81920];
                var memStream = new MemoryStream();
                long bytesRead = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await memStream.WriteAsync(buffer, 0, read, ct);
                    bytesRead += read;
                    progress.Report(bytesRead * 1.0 / totalBytes);
                }
                memStream.Position = 0;
                await client.Drives[driveId].Items["root"].ItemWithPath(normalizedPath).Content.PutAsync(memStream, cancellationToken: ct);
            }
            else
            {
                await client.Drives[driveId].Items["root"].ItemWithPath(normalizedPath).Content.PutAsync(content, cancellationToken: ct);
            }
        }

        public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to download files.");

            var client = GetClient();
            var driveId = await GetDriveIdAsync();
            var normalizedPath = NormalizePath(remotePath);

            var stream = await client.Drives[driveId].Items["root"].ItemWithPath(normalizedPath).Content.GetAsync(cancellationToken: ct);
            if (stream == null)
                throw new InvalidOperationException("File not found or could not be downloaded.");

            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (progress != null && stream.CanSeek)
            {
                var totalBytes = stream.Length;
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                var buffer = new byte[81920];
                long bytesDownloaded = 0;
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    bytesDownloaded += bytesRead;
                    progress.Report(bytesDownloaded * 1.0 / totalBytes);
                }
            }
            else
            {
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream, ct);
            }
        }

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            if (!_authService.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to delete files.");

            var client = GetClient();
            var driveId = await GetDriveIdAsync();
            var normalizedPath = NormalizePath(path);

            await client.Drives[driveId].Items["root"].ItemWithPath(normalizedPath).DeleteAsync();
        }
    }
}
