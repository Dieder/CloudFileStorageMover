using System.Net.Http.Json;
using System.Text.Json;
using FileMover.UI.Models;

namespace FileMover.UI.Services;

public class PrimeStorageProvider : IStorageProviderService
{
    public string ProviderType => "prime";

    private readonly HttpClient _httpClient;
    private readonly ProviderConfigLoader _configLoader;
    private ProviderConfig? _config;

    public PrimeStorageProvider(ProviderConfigLoader configLoader, HttpClient? httpClient = null)
    {
        _configLoader = configLoader;
        _httpClient = httpClient ?? new HttpClient();
    }

    private async Task EnsureConfigLoadedAsync()
    {
        if (_config is not null)
            return;

        _config = await _configLoader.LoadAsync("prime");
        if (_config is null)
            throw new InvalidOperationException("Prime storage provider configuration not found.");

        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl ?? "https://api.prime-storage.example.com/v1");
        if (!string.IsNullOrEmpty(_config.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _config.ApiKey);
        if (!string.IsNullOrEmpty(_config.ApiSecret))
            _httpClient.DefaultRequestHeaders.Add("X-Api-Secret", _config.ApiSecret);
    }

    public async Task<List<string>> ListFoldersAsync(string path)
    {
        await EnsureConfigLoadedAsync();
        var response = await _httpClient.GetAsync($"/folders?path={Uri.EscapeDataString(path)}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ListFolderResponse>();
        return result?.Folders ?? [];
    }

    public async Task<List<FileItem>> ListFilesAsync(string path)
    {
        await EnsureConfigLoadedAsync();
        var response = await _httpClient.GetAsync($"/files?path={Uri.EscapeDataString(path)}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ListFilesResponse>();
        return result?.Files?.Select(f => new FileItem
        {
            Id = f.Id ?? string.Empty,
            Name = f.Name ?? string.Empty,
            Path = f.Path ?? string.Empty,
            IsFolder = f.IsFolder,
            Size = f.Size,
            Modified = f.Modified,
            MimeType = f.MimeType
        }).ToList() ?? [];
    }

    public async Task CreateFileAsync(string path, Stream contentStream)
    {
        await EnsureConfigLoadedAsync();
        using var content = new StreamContent(contentStream);
        var response = await _httpClient.PostAsync($"/files?path={Uri.EscapeDataString(path)}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task UploadFileAsync(string localPath, string remotePath)
    {
        await EnsureConfigLoadedAsync();
        await using var stream = File.OpenRead(localPath);
        using var content = new StreamContent(stream);
        var response = await _httpClient.PostAsync($"/files?path={Uri.EscapeDataString(remotePath)}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task RenameAsync(string oldPath, string newName)
    {
        await EnsureConfigLoadedAsync();
        var payload = new { name = newName };
        var response = await _httpClient.PatchAsJsonAsync($"/files?path={Uri.EscapeDataString(oldPath)}", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string path)
    {
        await EnsureConfigLoadedAsync();
        var response = await _httpClient.DeleteAsync($"/files?path={Uri.EscapeDataString(path)}");
        response.EnsureSuccessStatusCode();
    }

    private class ListFolderResponse
    {
        public List<string> Folders { get; set; } = [];
    }

    private class ListFilesResponse
    {
        public List<FileEntry> Files { get; set; } = [];
    }

    private class FileEntry
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool IsFolder { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string? MimeType { get; set; }
    }
}
