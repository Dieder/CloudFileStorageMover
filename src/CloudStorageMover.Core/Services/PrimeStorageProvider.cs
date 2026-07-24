using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudStorageMover.Core.Services
{
    public class PrimeStorageProvider : IStorageProviderService
    {
        private const string GraphApiBaseUrl = "https://graph-node-api-production.up.railway.app/api/v1/media";
        private readonly PrimeSignInService _authService;

        public StorageProviderType ProviderType => StorageProviderType.Prime;
        public string DisplayName => "Prime";

        public PrimeStorageProvider(PrimeSignInService authService)
        {
            _authService = authService;
        }

        public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to list folders.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/user/media/folders{(!string.IsNullOrEmpty(path) ? $"/{path}" : "")}";
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrimeApiResponse>(responseBody);

            var folders = new List<FileItem>();
            if (apiResponse.Data != null)
            {
                foreach (var item in apiResponse.Data)
                {
                    if (item.Type == "folder" && item.StorageProvider == "aws")
                    {
                        var folderItem = new FileItem
                        {
                            Name = item.Name,
                            FullPath = item.Id,
                            IsDirectory = true,
                            ProviderType = ProviderType
                        };
                        folders.Add(folderItem);
                    }
                }
            }

            return folders;
        }

        public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to list files.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/user/media/files{(!string.IsNullOrEmpty(path) ? $"/folder/{path}" : "")}";
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrimeApiResponse>(responseBody);

            var files = new List<FileItem>();
            if (apiResponse.Data != null)
            {
                foreach (var item in apiResponse.Data)
                {
                    if (item.Type == "file" && item.StorageProvider == "aws")
                    {
                        var fileItem = new FileItem
                        {
                            Name = item.Name,
                            FullPath = item.Id,
                            Size = item.Size ?? 0,
                            ModifiedDate = DateTime.Parse(item.CreatedAt),
                            IsDirectory = false,
                            ProviderType = ProviderType,
                            IconPath = item.Thumbnail != null ? item.Thumbnail.Url : null
                        };
                        files.Add(fileItem);
                    }
                }
            }

            return files;
        }

        public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                return await Task.FromResult(new List<string>());
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/user/media/folders";
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrimeApiResponse>(responseBody);

            var commonDirs = new List<string>
            {
                "Prime Media Library"
            };

            if (apiResponse.Data != null)
            {
                commonDirs.AddRange(apiResponse.Data.Where(d => d.Type == "folder").Select(d => d.Name));
            }

            return commonDirs;
        }

        public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to open a file stream.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/user/media/download?path={Uri.EscapeDataString(remotePath)}";
            var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, ct);
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

            var requestUrl = $"{GraphApiBaseUrl}/user/media/upload";

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

            var multipartContent = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(uploadContent);
            multipartContent.Add(fileStreamContent, "file", Path.GetFileName(localPath));
            multipartContent.Add(new StringContent(remotePath), "path");

            var response = await client.PostAsync(requestUrl, multipartContent, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to download files.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"{GraphApiBaseUrl}/user/media/download?path={Uri.EscapeDataString(remotePath)}";
            var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

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

            var deleteUrl = $"{GraphApiBaseUrl}/user/media/{path}";
            var response = await client.DeleteAsync(deleteUrl, ct);
            response.EnsureSuccessStatusCode();
        }

        private bool IsSignedIn()
        {
            return _authService.IsSignedIn;
        }

        private async Task<string> GetCurrentAccessTokenAsync()
        {
            var accessToken = await _authService.GetSecureValueAsync("prime_access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token not available.");
            }
            return accessToken;
        }
    }

    public class PrimeApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<PrimeMediaItem> Data { get; set; } = new();
    }

    public class PrimeMediaItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("storage_provider")]
        public string StorageProvider { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("thumbnail")]
        public ThumbnailInfo Thumbnail { get; set; } = new();
    }

    public class ThumbnailInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}