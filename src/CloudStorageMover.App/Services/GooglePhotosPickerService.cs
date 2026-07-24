using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudStorageMover.Core.Services;

namespace CloudStorageMover.App.Services;

public class GooglePhotosPickerService
{
    private const string PickerScope = "https://www.googleapis.com/auth/photospicker.mediaitems.readonly";
    private const int PollTimeoutSeconds = 120;
    private const int PollIntervalMs = 2000;

    private readonly GooglePhotosSignInService _authService;
    private readonly AppSettingsService _settings;
    private readonly IOAuthLoopbackService _oauthLoopback;

    public GooglePhotosPickerService(
        GooglePhotosSignInService authService,
        AppSettingsService settings,
        IOAuthLoopbackService oauthLoopback)
    {
        _authService = authService;
        _settings = settings;
        _oauthLoopback = oauthLoopback;
    }

    public async Task<PickerFlowResult> RunPickerAsync(string downloadFolder, CancellationToken ct = default)
    {
        if (!await EnsureSignedInAsync())
            return PickerFlowResult.AuthFailed("Could not authenticate with Google Photos.");

        var session = await CreateSessionAsync(ct);
        if (session == null || string.IsNullOrEmpty(session.Id))
            return PickerFlowResult.Failed("Failed to create picker session.");

        if (!string.IsNullOrEmpty(session.PickerUri))
            await Launcher.OpenAsync(new Uri(session.PickerUri));

        var mediaItems = await PollForSelectionAsync(session.Id, ct);
        if (mediaItems == null || mediaItems.Count == 0)
            return PickerFlowResult.Empty("No photos were selected.");

        var downloaded = await DownloadAllAsync(mediaItems, downloadFolder, ct);
        return PickerFlowResult.Success(downloaded, mediaItems.Count);
    }

    private async Task<bool> EnsureSignedInAsync()
    {
        if (_authService.IsSignedIn && !_authService.IsTokenExpired)
            return true;

        var clientId = _settings.GooglePhotosClientId;
        var secret = _settings.GooglePhotosSecret;
        if (string.IsNullOrEmpty(clientId))
            return false;

        if (_authService.HasRefreshToken)
        {
            var refreshed = await _authService.EnsureTokenAsync(clientId, secret);
            if (refreshed && _authService.IsSignedIn)
                return true;
        }

        return await AuthenticateWithPickerScopeAsync(clientId, secret);
    }

    private async Task<bool> AuthenticateWithPickerScopeAsync(string clientId, string? secret)
    {
        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString("http://localhost")}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(PickerScope)}&" +
            $"access_type=offline&prompt=consent";

        var result = await _oauthLoopback.StartAsync(authUrl);
        if (!string.IsNullOrEmpty(result.Error) || string.IsNullOrEmpty(result.Code))
            return false;

        using var httpClient = new HttpClient();
        var form = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = secret ?? "",
            ["redirect_uri"] = result.RedirectUri ?? "http://localhost",
            ["grant_type"] = "authorization_code",
            ["code"] = result.Code
        };

        var tokenResponse = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(form));

        if (!tokenResponse.IsSuccessStatusCode)
            return false;

        var json = await tokenResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var accessToken = root.GetProperty("access_token").GetString()!;
        var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt64() : 3600;

        _authService.SetTokens(accessToken, refreshToken, expiresIn);
        return _authService.IsSignedIn;
    }

    private async Task<PickerSession?> CreateSessionAsync(CancellationToken ct)
    {
        var clientId = _settings.GooglePhotosClientId;
        var secret = _settings.GooglePhotosSecret;
        if (_authService.IsTokenExpired)
            await _authService.EnsureTokenAsync(clientId, secret);

        var token = _authService.AccessToken;
        if (string.IsNullOrEmpty(token))
            return null;

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://photospicker.googleapis.com/v1/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            System.Diagnostics.Debug.WriteLine($"[Picker] Session creation failed: {response.StatusCode} {json}");
            return null;
        }

        return JsonSerializer.Deserialize<PickerSession>(json, JsonDefaults.CamelCase);
    }

    private async Task<List<PickerMediaItem>> PollForSelectionAsync(string sessionId, CancellationToken ct)
    {
        var allItems = new List<PickerMediaItem>();
        var deadline = DateTime.UtcNow.AddSeconds(PollTimeoutSeconds);
        string? pageToken = null;

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(PollIntervalMs, ct);

            var items = await GetMediaItemsAsync(sessionId, pageToken, ct);
            if (items?.MediaItems != null)
            {
                allItems.AddRange(items.MediaItems);

                if (!string.IsNullOrEmpty(items.NextPageToken))
                {
                    pageToken = items.NextPageToken;
                    continue;
                }

                if (allItems.Count > 0)
                    return allItems;
            }

            if (items?.MediaItems == null || items.MediaItems.Count == 0)
            {
                if (pageToken == null)
                    continue;
                break;
            }
        }

        return allItems;
    }

    private async Task<PickerMediaItemsResponse?> GetMediaItemsAsync(string sessionId, string? pageToken, CancellationToken ct)
    {
        var query = $"sessionId={Uri.EscapeDataString(sessionId)}&pageSize=100";
        if (!string.IsNullOrEmpty(pageToken))
            query += $"&pageToken={Uri.EscapeDataString(pageToken)}";

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://photospicker.googleapis.com/v1/mediaItems?{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);

        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<PickerMediaItemsResponse>(json, JsonDefaults.CamelCase);
    }

    private async Task<int> DownloadAllAsync(List<PickerMediaItem> items, string downloadFolder, CancellationToken ct)
    {
        Directory.CreateDirectory(downloadFolder);
        var downloaded = 0;

        using var downloadClient = new HttpClient();

        foreach (var item in items)
        {
            var baseUrl = item.MediaFile?.BaseUrl;
            var filename = item.MediaFile?.Filename;
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(filename))
                continue;

            try
            {
                var url = item.MimeType?.StartsWith("video/") == true
                    ? $"{baseUrl}=dv"
                    : $"{baseUrl}=d";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);

                var response = await downloadClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[Picker] Download failed for {filename}: {response.StatusCode}");
                    continue;
                }

                var filePath = Path.Combine(downloadFolder, filename);
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream, ct);
                downloaded++;
                System.Diagnostics.Debug.WriteLine($"[Picker] Downloaded {filename}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Picker] Download error for {filename}: {ex.Message}");
                continue;
            }
        }

        return downloaded;
    }

    private static class JsonDefaults
    {
        public static readonly JsonSerializerOptions CamelCase = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}

public class PickerSession
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("pickerUri")]
    public string? PickerUri { get; set; }

    [JsonPropertyName("mediaItemsSet")]
    public bool? MediaItemsSet { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class PickerMediaItemsResponse
{
    [JsonPropertyName("mediaItems")]
    public List<PickerMediaItem>? MediaItems { get; set; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public class PickerMediaItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("mediaFile")]
    public PickerMediaFile? MediaFile { get; set; }
}

public class PickerMediaFile
{
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

public class PickerFlowResult
{
    public bool IsSuccess { get; init; }
    public int DownloadedCount { get; init; }
    public int TotalCount { get; init; }
    public string Message { get; init; } = string.Empty;

    public static PickerFlowResult Success(int downloaded, int total) =>
        new() { IsSuccess = true, DownloadedCount = downloaded, TotalCount = total,
            Message = $"Downloaded {downloaded} of {total} photos." };

    public static PickerFlowResult Empty(string message) =>
        new() { IsSuccess = false, Message = message };

    public static PickerFlowResult AuthFailed(string message) =>
        new() { IsSuccess = false, Message = message };

    public static PickerFlowResult Failed(string message) =>
        new() { IsSuccess = false, Message = message };
}
