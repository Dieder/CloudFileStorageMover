using System.Net.Http.Json;
using System.Text.Json;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Services;

public class PCloudStorageProvider : IStorageProviderService
{
    private readonly PCloudSignInService _authService;
    private readonly HttpClient _http;

    public StorageProviderType ProviderType => StorageProviderType.PCloud;
    public string DisplayName => "pCloud";

    public PCloudStorageProvider(PCloudSignInService authService)
    {
        _authService = authService;
        _http = new HttpClient { BaseAddress = new Uri("https://api.pcloud.com/") };
    }

    public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var json = await CallApiAsync($"listfolder?path={Uri.EscapeDataString(path)}", ct);
        if (!TryGetMetadata(json, out var metadata)) return new List<FileItem>();

        var folders = new List<FileItem>();
        if (metadata.TryGetProperty("contents", out var contents))
        {
            foreach (var item in contents.EnumerateArray())
            {
                if (item.TryGetProperty("isfolder", out var isFolder) && isFolder.GetInt32() == 1)
                {
                    folders.Add(ParseFileItem(item));
                }
            }
        }
        return folders;
    }

    public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var json = await CallApiAsync($"listfolder?path={Uri.EscapeDataString(path)}", ct);
        if (!TryGetMetadata(json, out var metadata)) return new List<FileItem>();

        var files = new List<FileItem>();
        if (metadata.TryGetProperty("contents", out var contents))
        {
            foreach (var item in contents.EnumerateArray())
            {
                if (item.TryGetProperty("isfolder", out var isFolder) && isFolder.GetInt32() == 0)
                {
                    files.Add(ParseFileItem(item));
                }
            }
        }
        return files;
    }

    public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        return new List<string> { "/" };
    }

    public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var response = await _http.GetAsync(
            $"downloadfile?path={Uri.EscapeDataString(remotePath)}&auth={_authService.AccessToken}",
            HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(ct);
    }

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var remoteDir = Path.GetDirectoryName(remotePath)?.Replace('\\', '/') ?? "/";
        var fileName = Path.GetFileName(remotePath);

        using var formContent = new MultipartFormDataContent();
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
            formContent.Add(new StreamContent(memStream), "filename", fileName);
        }
        else
        {
            formContent.Add(new StreamContent(content), "filename", fileName);
        }

        var response = await _http.PostAsync(
            $"uploadfile?path={Uri.EscapeDataString(remoteDir)}&auth={_authService.AccessToken}",
            formContent, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var response = await _http.GetAsync(
            $"downloadfile?path={Uri.EscapeDataString(remotePath)}&auth={_authService.AccessToken}",
            HttpCompletionOption.ResponseHeadersRead, ct);
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
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        EnsureSignedIn();
        var json = await CallApiAsync($"deletefile?path={Uri.EscapeDataString(path)}", ct);
        var result = json.TryGetProperty("result", out var r) ? r.GetInt32() : -1;
        if (result != 0)
            throw new InvalidOperationException($"pCloud delete failed with result: {result}");
    }

    private void EnsureSignedIn()
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to access pCloud.");
    }

    private async Task<JsonElement> CallApiAsync(string endpoint, CancellationToken ct)
    {
        var response = await _http.GetAsync($"{endpoint}&auth={_authService.AccessToken}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement.Clone();

        if (root.TryGetProperty("result", out var result) && result.GetInt32() != 0)
        {
            var error = root.TryGetProperty("error", out var err) ? err.GetString() : "Unknown error";
            throw new InvalidOperationException($"pCloud API error: {error}");
        }

        return root;
    }

    private static bool TryGetMetadata(JsonElement json, out JsonElement metadata)
    {
        if (json.TryGetProperty("metadata", out metadata))
            return true;
        metadata = default;
        return false;
    }

    private static FileItem ParseFileItem(JsonElement item)
    {
        var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
        var path = item.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";
        var size = item.TryGetProperty("size", out var s) ? s.GetInt64() : 0L;
        var isFolder = item.TryGetProperty("isfolder", out var f) && f.GetInt32() == 1;

        DateTime modified = DateTime.MinValue;
        if (item.TryGetProperty("modified", out var mod))
        {
            long unix = mod.GetInt64();
            modified = DateTimeOffset.FromUnixTimeSeconds(unix).DateTime;
        }

        return new FileItem
        {
            Name = name,
            FullPath = path,
            Size = size,
            ModifiedDate = modified,
            IsDirectory = isFolder,
            ProviderType = StorageProviderType.PCloud
        };
    }
}
