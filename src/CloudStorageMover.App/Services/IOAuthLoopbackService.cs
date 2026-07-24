namespace CloudStorageMover.App.Services;

public record OAuthLoopbackResult(string? Code, string? Error, string? RedirectUri);

public interface IOAuthLoopbackService
{
    Task<OAuthLoopbackResult> StartAsync(string authUrl, CancellationToken ct = default);
}
