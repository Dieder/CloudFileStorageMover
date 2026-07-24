using Custom.Google.Services.Models;

namespace Custom.Google.Services.Services;

public interface IGooglePhotosPickerService
{
    Task<PhotosPickerSession> CreateSessionAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<PhotosPickerSession> GetSessionAsync(string accessToken, string sessionId, CancellationToken cancellationToken = default);
    Task<MediaItemsResponse> GetMediaItemsAsync(string accessToken, string sessionId, int pageSize = 25, string? pageToken = null, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadImageAsync(string accessToken, string baseUrl, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadVideoAsync(string accessToken, string baseUrl, CancellationToken cancellationToken = default);
}
