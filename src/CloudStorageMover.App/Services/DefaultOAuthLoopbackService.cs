using Microsoft.Maui.Authentication;

namespace CloudStorageMover.App.Services;

public class DefaultOAuthLoopbackService : IOAuthLoopbackService
{
    public async Task<OAuthLoopbackResult> StartAsync(string authUrl, CancellationToken ct = default)
    {
        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri("http://localhost"));

            var code = authResult?.Properties.GetValueOrDefault("code");
            if (string.IsNullOrEmpty(code))
                code = authResult?.Properties.GetValueOrDefault("authorization_code");

            return new OAuthLoopbackResult(code, authResult?.Properties.GetValueOrDefault("error"), "http://localhost");
        }
        catch (TaskCanceledException)
        {
            return new OAuthLoopbackResult(null, "cancelled", "http://localhost");
        }
        catch (Exception ex)
        {
            return new OAuthLoopbackResult(null, ex.Message, "http://localhost");
        }
    }
}
