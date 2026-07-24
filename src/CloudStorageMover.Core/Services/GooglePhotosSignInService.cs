using System.Text.Json;

namespace CloudStorageMover.Core.Services;

public class GooglePhotosSignInService
{
    private const string AccessTokenKey = "gphotos_access_token";
    private const string RefreshTokenKey = "gphotos_refresh_token";
    private const string TokenExpiryKey = "gphotos_token_expiry";

    private readonly Dictionary<string, string?> _store = new();

    public bool IsSignedIn =>
        _store.TryGetValue(AccessTokenKey, out var token) &&
        !string.IsNullOrEmpty(token);

    public bool HasRefreshToken =>
        _store.TryGetValue(RefreshTokenKey, out var rt) &&
        !string.IsNullOrEmpty(rt);

    public bool IsTokenExpired
    {
        get
        {
            if (_store.TryGetValue(TokenExpiryKey, out var expiry) &&
                !string.IsNullOrEmpty(expiry) &&
                long.TryParse(expiry, out var unixExpiry))
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > unixExpiry - 60;
            }
            return true;
        }
    }

    public string? AccessToken =>
        _store.TryGetValue(AccessTokenKey, out var token) ? token : null;

    public void SetTokens(string accessToken, string? refreshToken, long expiresIn)
    {
        _store[AccessTokenKey] = accessToken;
        if (!string.IsNullOrEmpty(refreshToken))
            _store[RefreshTokenKey] = refreshToken;
        _store[TokenExpiryKey] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn).ToString();
    }

    public async Task<bool> EnsureTokenAsync(string clientId, string? clientSecret = null)
    {
        if (!IsTokenExpired && IsSignedIn)
            return true;

        if (HasRefreshToken)
            return await RefreshTokenAsync(clientId, clientSecret);

        return IsSignedIn;
    }

    public async Task<bool> RefreshTokenAsync(string clientId, string? clientSecret = null)
    {
        if (!_store.TryGetValue(RefreshTokenKey, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return false;

        try
        {
            using var client = new HttpClient();
            var form = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };
            if (!string.IsNullOrEmpty(clientSecret))
                form["client_secret"] = clientSecret;

            using var response = await client.PostAsync(
                "https://oauth2.googleapis.com/token", new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var newAccessToken = root.GetProperty("access_token").GetString()!;
            long expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt64() : 3600;

            SetTokens(newAccessToken, null, expiresIn);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SignOut()
    {
        _store.Clear();
    }
}
