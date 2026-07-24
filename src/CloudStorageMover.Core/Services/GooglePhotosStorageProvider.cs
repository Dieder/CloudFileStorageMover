using System.Net.Http.Headers;
using System.Text.Json;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Services;

public class GooglePhotosStorageProvider : IStorageProviderService
{
    private const string ApiBase = "https://photoslibrary.googleapis.com/v1/";
    private const string RootAlbumId = "__root__";

    private readonly GooglePhotosSignInService _auth;
    private readonly AppSettingsService _settings;
    private readonly HttpClient _http;

    public StorageProviderType ProviderType => StorageProviderType.GooglePhotos;
    public string DisplayName => "Google Photos";

    public GooglePhotosStorageProvider(GooglePhotosSignInService authService, AppSettingsService settings)
    {
        _auth = authService;
        _settings = settings;
        _http = new HttpClient { BaseAddress = new Uri(ApiBase) };
    }

    public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        await EnsureSignedInAsync();

        if (path == RootAlbumId || string.IsNullOrEmpty(path))
        {
            return await ListAlbumsAsync(ct);
        }

        return new List<FileItem>();
    }

    public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        await EnsureSignedInAsync();

        if (path == RootAlbumId || string.IsNullOrEmpty(path))
        {
            return await ListUngroupedMediaAsync(ct);
        }

        return await ListAlbumMediaAsync(path, ct);
    }

    public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        return new List<string> { $"{RootAlbumId}|Google Photos" };
    }

    public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        await EnsureSignedInAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, remotePath);
        SetAuthHeader(request);
        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _auth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (refreshed)
            {
                request = new HttpRequestMessage(HttpMethod.Get, remotePath);
                SetAuthHeader(request);
                response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            }
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(ct);
    }

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        throw new NotSupportedException("Google Photos does not support uploading via the public API.");
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        await EnsureSignedInAsync();

        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var response = await _http.GetAsync(remotePath, HttpCompletionOption.ResponseHeadersRead, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            var refreshed = await _auth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (refreshed)
            {
                response = await _http.GetAsync(remotePath, HttpCompletionOption.ResponseHeadersRead, ct);
            }
        }

        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

        if (progress != null && totalBytes.HasValue)
        {
            var buffer = new byte[81920];
            long bytesDownloaded = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                bytesDownloaded += bytesRead;
                progress.Report(bytesDownloaded * 1.0 / totalBytes.Value);
            }
        }
        else
        {
            await stream.CopyToAsync(fileStream, ct);
        }

        response.Dispose();
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        throw new NotSupportedException("Google Photos does not support deletion via the public API.");
    }

    private async Task EnsureSignedInAsync()
    {
        if (!_auth.IsSignedIn)
        {
            var refreshed = await _auth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (!refreshed || !_auth.IsSignedIn)
                throw new InvalidOperationException("User must be signed in to access Google Photos.");
        }
    }

    private void SetAuthHeader(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
    }

    private async Task<JsonElement> GetAsync(string endpoint, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        SetAuthHeader(request);
        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _auth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (refreshed)
            {
                request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                SetAuthHeader(request);
                response = await _http.SendAsync(request, ct);
            }
        }

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private async Task<JsonElement> PostAsync(string endpoint, object body, CancellationToken ct)
    {
        var bodyJson = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json")
        };
        SetAuthHeader(request);
        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _auth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (refreshed)
            {
                request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json")
                };
                SetAuthHeader(request);
                response = await _http.SendAsync(request, ct);
            }
        }

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private async Task<List<FileItem>> ListAlbumsAsync(CancellationToken ct)
    {
        var items = new List<FileItem>();
        string? pageToken = null;

        do
        {
            var endpoint = "albums?pageSize=50";
            if (!string.IsNullOrEmpty(pageToken))
                endpoint += $"&pageToken={pageToken}";

            var root = await GetAsync(endpoint, ct);

            if (root.TryGetProperty("albums", out var albums))
            {
                foreach (var album in albums.EnumerateArray())
                {
                    var id = album.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                    var title = album.TryGetProperty("title", out var t) ? t.GetString() ?? "Untitled" : "Untitled";
                    var itemCount = album.TryGetProperty("mediaItemsCount", out var c) ? c.GetString() ?? "0" : "0";

                    items.Add(new FileItem
                    {
                        Name = title,
                        FullPath = id,
                        IsDirectory = true,
                        ProviderType = ProviderType
                    });
                }
            }

            pageToken = root.TryGetProperty("nextPageToken", out var token) ? token.GetString() : null;
        } while (!string.IsNullOrEmpty(pageToken));

        return items;
    }

    private async Task<List<FileItem>> ListAlbumMediaAsync(string albumId, CancellationToken ct)
    {
        return await SearchMediaAsync(new { albumId, pageSize = 100 }, ct);
    }

    private async Task<List<FileItem>> ListUngroupedMediaAsync(CancellationToken ct)
    {
        return await SearchMediaAsync(new { pageSize = 100 }, ct);
    }

    private async Task<List<FileItem>> SearchMediaAsync(object body, CancellationToken ct)
    {
        var items = new List<FileItem>();
        string? pageToken = null;

        do
        {
            var requestBody = pageToken != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(body))!
                : new Dictionary<string, object>();

            if (pageToken != null)
                requestBody["pageToken"] = pageToken;

            var root = await PostAsync("mediaItems:search", requestBody, ct);

            if (root.TryGetProperty("mediaItems", out var mediaItems))
            {
                foreach (var item in mediaItems.EnumerateArray())
                {
                    var fileItem = ParseMediaItem(item);
                    if (fileItem != null)
                        items.Add(fileItem);
                }
            }

            pageToken = root.TryGetProperty("nextPageToken", out var token) ? token.GetString() : null;
        } while (!string.IsNullOrEmpty(pageToken));

        return items;
    }

    private static FileItem? ParseMediaItem(JsonElement item)
    {
        var id = item.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
        var filename = item.TryGetProperty("filename", out var f) ? f.GetString() ?? "" : "";
        var mimeType = item.TryGetProperty("mimeType", out var m) ? m.GetString() ?? "" : "";

        long size = 0;
        if (item.TryGetProperty("mediaMetadata", out var meta))
        {
            if (meta.TryGetProperty("width", out var w) && meta.TryGetProperty("height", out var h))
                size = w.GetInt64() * h.GetInt64() * 3;
        }

        string? downloadUrl = null;
        if (item.TryGetProperty("baseUrl", out var baseUrl))
        {
            downloadUrl = baseUrl.GetString() + "=d";
        }

        DateTime modified = DateTime.MinValue;
        if (item.TryGetProperty("mediaMetadata", out var meta2))
        {
            if (meta2.TryGetProperty("creationTime", out var created))
            {
                DateTime.TryParse(created.GetString(), out modified);
            }
        }

        return new FileItem
        {
            Name = filename,
            FullPath = downloadUrl ?? id,
            Size = size,
            ModifiedDate = modified,
            IsDirectory = false,
            ProviderType = StorageProviderType.GooglePhotos
        };
    }
}
