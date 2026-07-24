using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Custom.Google.Services.Models;

namespace Custom.Google.Services.Services;

public sealed class GooglePhotosPickerService(HttpClient httpClient) : IGooglePhotosPickerService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PhotosPickerSession> CreateSessionAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://photospicker.googleapis.com/v1/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PhotosPickerSession>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Unable to deserialize picker session response.");
    }

    public async Task<PhotosPickerSession> GetSessionAsync(string accessToken, string sessionId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://photospicker.googleapis.com/v1/sessions/{sessionId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PhotosPickerSession>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Unable to deserialize picker session response.");
    }

    public async Task<MediaItemsResponse> GetMediaItemsAsync(string accessToken, string sessionId, int pageSize = 25, string? pageToken = null, CancellationToken cancellationToken = default)
    {
        var queryBuilder = new StringBuilder($"sessionId={Uri.EscapeDataString(sessionId)}&pageSize={pageSize}");
        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            queryBuilder.Append($"&pageToken={Uri.EscapeDataString(pageToken)}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://photospicker.googleapis.com/v1/mediaItems?{queryBuilder}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<MediaItemsResponse>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Unable to deserialize media items response.");
    }

    public async Task<byte[]> DownloadImageAsync(string accessToken, string baseUrl, CancellationToken cancellationToken = default)
    {
        return await DownloadAsync(accessToken, baseUrl, cancellationToken);
    }

    public async Task<byte[]> DownloadVideoAsync(string accessToken, string baseUrl, CancellationToken cancellationToken = default)
    {
        return await DownloadAsync(accessToken, $"{baseUrl}=dv", cancellationToken);
    }

    private async Task<byte[]> DownloadAsync(string accessToken, string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
