using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Text.Json;

namespace Custom.Google.Services;

public sealed class SignInService
{
    private readonly string? _clientId;
    private readonly string? _clientSecret;
    private readonly string _redirectUri;

    public SignInService(IConfiguration configuration, string? redirectUri = null)
    {
        _clientId = configuration["googleauthsettings:clientId"];
        _clientSecret = configuration["googleauthsettings:secret"];
        _redirectUri = string.IsNullOrWhiteSpace(redirectUri)
            ? configuration["googleauthsettings:RedirectUri"] ?? "http://localhost"
            : redirectUri;
    }

    public async Task<GoogleSignInResult> LoginWithGoogleAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_clientId) || _clientId.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google client ID is not configured. Set GoogleAuth:ClientId in appsettings.json or pass it to the SignInService constructor.");
        }

        var scope = Uri.EscapeDataString("openid email profile");
        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(_clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
            "&response_type=code" +
            $"&scope={scope}" +
            "&access_type=offline&prompt=consent";

        var authResult = await WebAuthenticator.AuthenticateAsync(
            new WebAuthenticatorOptions
            {
                Url = new Uri(authUrl),
                CallbackUrl = new Uri(_redirectUri)
            });

        var code = authResult?.Properties.GetValueOrDefault("code");
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Google authentication did not return an authorization code.");
        }

        var tokenResponse = await ExchangeCodeForTokensAsync(code, cancellationToken);

        await SaveTokenAsync("google_access_token", tokenResponse.AccessToken);
        if (!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
        {
            await SaveTokenAsync("google_refresh_token", tokenResponse.RefreshToken);
        }

        return tokenResponse;
    }

    private async Task<string> RefreshGoogleToken(string refreshToken)
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _clientId!,
            ["client_secret"] = _clientSecret!,   // needed for refresh
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        return result.GetProperty("access_token").GetString();
    }

    private async Task<GoogleSignInResult> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var form = new Dictionary<string, string>
        {
            ["client_id"] = _clientId!,
            ["client_secret"] = _clientSecret ?? string.Empty,
            ["redirect_uri"] = _redirectUri,
            ["grant_type"] = "authorization_code",
            ["code"] = code
        };

        using var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(form), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Google token exchange failed: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        return new GoogleSignInResult(
            AccessToken: root.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken: root.TryGetProperty("refresh_token", out var refreshToken) ? refreshToken.GetString() : null,
            IdToken: root.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null,
            ExpiresIn: root.TryGetProperty("expires_in", out var expiresIn) && expiresIn.TryGetInt64(out var expires) ? expires : 3600);
    }

    private async Task SaveTokenAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
        }
        catch
        {
            Preferences.Set(key, value);
        }
    }
}
public sealed record GoogleSignInResult(string AccessToken, string? RefreshToken, string? IdToken, long ExpiresIn);