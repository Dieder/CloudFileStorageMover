using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CloudStorageMover.Core.Services
{
    public class GoogleDriveStorageProvider : IStorageProviderService
    {
        private const string RootFolderId = "root";
        private readonly GoogleDriveSignInService _authService;

        public StorageProviderType ProviderType => StorageProviderType.GoogleDrive;
        public string DisplayName => "Google Drive";

        public GoogleDriveStorageProvider(GoogleDriveSignInService authService)
        {
            _authService = authService;
        }

        public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
        {
            var folderId = string.IsNullOrEmpty(path) ? RootFolderId : path;
            var items = await ListFilesInternalAsync(folderId, onlyFolders: true, ct);
            return items;
        }

        public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
        {
            var folderId = string.IsNullOrEmpty(path) ? RootFolderId : path;
            var items = await ListFilesInternalAsync(folderId, onlyFolders: false, ct);
            return items.Where(f => !f.IsDirectory).ToList();
        }

        public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
        {
            return await Task.FromResult(new List<string> { $"{RootFolderId}|Google Drive" });
        }

        public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to open a file stream.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"https://www.googleapis.com/drive/v3/files/{remotePath}?alt=media";
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
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var fileMetadata = new
            {
                name = Path.GetFileName(remotePath)
            };

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

            var jsonContent = new StringContent(JsonSerializer.Serialize(fileMetadata), System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var requestContent = new MultipartFormDataContent();
            requestContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");

            requestContent.Add(jsonContent, "metadata");
            requestContent.Add(new StreamContent(uploadContent), "media", Path.GetFileName(localPath));

            var baseUrl = "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart";
            if (!string.IsNullOrEmpty(remotePath))
            {
                ((MultipartFormDataContent)requestContent).Add(new StringContent(remotePath), "parents");
            }

            var response = await client.PostAsync(baseUrl, requestContent, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            var fileResponse = JsonSerializer.Deserialize<GoogleDriveFileResponse>(responseBody);
        }

        public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!IsSignedIn())
            {
                throw new InvalidOperationException("User must be signed in to download files.");
            }

            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"https://www.googleapis.com/drive/v3/files/{remotePath}?alt=media";
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
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var deleteUrl = $"https://www.googleapis.com/drive/v3/files/{path}";
            var response = await client.DeleteAsync(deleteUrl, ct);
            response.EnsureSuccessStatusCode();
        }

        private bool IsSignedIn()
        {
            return _authService.IsSignedIn;
        }

        private async Task<string> GetCurrentAccessTokenAsync()
        {
            var accessToken = await _authService.GetSecureValueAsync("google_access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Authentication token not available.");
            }
            return accessToken;
        }

        private async Task<List<FileItem>> ListFilesInternalAsync(string folderId, bool onlyFolders, CancellationToken ct = default)
        {
            var accessToken = await GetCurrentAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var query = $"'{folderId}' in parents";
            if (onlyFolders)
            {
                query += " and mimeType='application/vnd.google-apps.folder'";
            }
            else
            {
                query += " and mimeType!='application/vnd.google-apps.folder'";
            }

            var requestUrl = $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(query)}&fields=files(id,name,mimeType)";
            var response = await client.GetAsync(requestUrl, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            var listResponse = JsonSerializer.Deserialize<GoogleDriveListResponse>(responseBody);

            var fileItems = new List<FileItem>();
            if (listResponse?.Files != null)
            {
                foreach (var file in listResponse.Files)
                {
                    fileItems.Add(new FileItem
                    {
                        Name = file.Name,
                        FullPath = file.Id,
                        IsDirectory = file.MimeType == "application/vnd.google-apps.folder",
                        ProviderType = ProviderType
                    });
                }
            }

            return fileItems;
        }
    }

    public class GoogleDriveFileResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;
    }

    public class GoogleDriveListResponse
    {
        [JsonPropertyName("files")]
        public List<GoogleDriveFileResponse> Files { get; set; } = new();
    }
}