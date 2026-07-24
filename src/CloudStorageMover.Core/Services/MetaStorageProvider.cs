using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CloudStorageMover.Core.Services
{
    public class MetaStorageProvider : IStorageProviderService
    {
        private const string GraphApiBaseUrl = "https://graph.facebook.com/v19.0";
        private readonly MetaSignInService _authService;

        public StorageProviderType ProviderType => StorageProviderType.Meta;
        public string DisplayName => "Meta";

        public MetaStorageProvider(MetaSignInService authService)
        {
            _authService = authService;
        }

        public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
        {
            if (!IsSignedIn())
                throw new InvalidOperationException("User must be signed in to list folders.");

            var accessToken = await GetCurrentAccessTokenAsync();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = string.IsNullOrEmpty(path)
                ? $"{GraphApiBaseUrl}/me/albums?limit=100"
                : $"{GraphApiBaseUrl}/me/albums?limit=100";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<GraphDataResponse<GraphAlbumItem>>(body);

            var folders = new List<FileItem>();
            if (data?.Data != null)
            {
                foreach (var album in data.Data)
                {
                    folders.Add(new FileItem
                    {
                        Name = album.Name,
                        FullPath = album.Id,
                        IsDirectory = true,
                        ProviderType = ProviderType
                    });
                }
            }
            return folders;
        }

        public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
        {
            if (!IsSignedIn())
                throw new InvalidOperationException("User must be signed in to list files.");

            var accessToken = await GetCurrentAccessTokenAsync();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = string.IsNullOrEmpty(path)
                ? $"{GraphApiBaseUrl}/me/photos?limit=100&fields=id,name,created_time,images"
                : $"{GraphApiBaseUrl}/{path}/photos?limit=100&fields=id,name,created_time,images";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<GraphDataResponse<GraphMediaItem>>(body);

            var files = new List<FileItem>();
            if (data?.Data != null)
            {
                foreach (var item in data.Data)
                {
                    files.Add(new FileItem
                    {
                        Name = !string.IsNullOrEmpty(item.Name) ? item.Name : $"photo_{item.Id}",
                        FullPath = item.Id,
                        Size = 0,
                        ModifiedDate = DateTime.TryParse(item.CreatedTime, out var dt) ? dt : DateTime.MinValue,
                        IsDirectory = false,
                        ProviderType = ProviderType,
                        IconPath = item.Images?.FirstOrDefault()?.Source
                    });
                }
            }
            return files;
        }

        public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
        {
            if (!IsSignedIn())
                return new List<string>();

            var accessToken = await GetCurrentAccessTokenAsync();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{GraphApiBaseUrl}/me/albums?limit=100&fields=id,name";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<GraphDataResponse<GraphAlbumItem>>(body);

            var commonDirs = new List<string>();
            if (data?.Data != null)
            {
                foreach (var album in data.Data)
                {
                    commonDirs.Add(album.Name);
                }
            }
            return commonDirs;
        }

        private async Task<string> ResolveMediaUrlAsync(string photoId, CancellationToken ct)
        {
            var accessToken = await GetCurrentAccessTokenAsync();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{GraphApiBaseUrl}/{photoId}?fields=images&access_token={accessToken}";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var photo = JsonSerializer.Deserialize<GraphMediaItem>(body);
            return photo?.Images?.FirstOrDefault()?.Source ?? photoId;
        }

        public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
        {
            if (!IsSignedIn())
                throw new InvalidOperationException("User must be signed in to open a file stream.");

            var mediaUrl = await ResolveMediaUrlAsync(remotePath, ct);
            using var client = new HttpClient();
            var response = await client.GetAsync(mediaUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(ct);
        }

        public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to upload files.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/me/photos/{remotePath}/videos";

            var boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString();
            var multiPartContent = new MultipartFormDataContent(boundary);

            Stream uploadContent = content;
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
                uploadContent = memStream;
            }

            var fileStreamContent = new StreamContent(uploadContent);
            multiPartContent.Add(fileStreamContent, "file", Path.GetFileName(localPath));

            var response = await client.PostAsync(requestUrl, multiPartContent, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!IsSignedIn())
                throw new InvalidOperationException("User must be signed in to download files.");

            var mediaUrl = await ResolveMediaUrlAsync(remotePath, ct);
            using var client = new HttpClient();
            var response = await client.GetAsync(mediaUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

            if (progress != null && totalBytes > 0)
            {
                var buffer = new byte[81920];
                long bytesDownloaded = 0;
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    bytesDownloaded += bytesRead;
                    progress.Report(bytesDownloaded * 1.0 / totalBytes);
                }
            }
            else
            {
                await contentStream.CopyToAsync(fileStream, ct);
            }
        }

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to delete files.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var deleteUrl = $"{GraphApiBaseUrl}/me/photos/{path}";
            var response = await client.DeleteAsync(deleteUrl, ct);
            response.EnsureSuccessStatusCode();
        }

        private bool IsSignedIn()
        {
            return _authService.IsSignedIn;
        }

        private async Task<string> GetCurrentAccessTokenAsync()
        {
            var accessToken = await _authService.GetSecureValueAsync("meta_access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token not available.");
            }
            return accessToken;
        }
    }

    public class GraphDataResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = new();
    }

    public class GraphAlbumItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class GraphMediaItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("created_time")]
        public string CreatedTime { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<GraphImageInfo> Images { get; set; } = new();
    }

    public class GraphImageInfo
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }
}