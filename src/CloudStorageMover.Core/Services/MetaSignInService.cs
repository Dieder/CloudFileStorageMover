using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CloudStorageMover.Core.Services
{
    public class MetaSignInService
    {
        private const string AccessTokenKey = "meta_access_token";
        private const string RefreshTokenKey = "meta_refresh_token";
        private const string EmailKey = "meta_email";
        private const string TokenExpiryKey = "meta_token_expiry";

        private readonly Dictionary<string, object> _settingsStore = new Dictionary<string, object>();
        private bool _isInitialized = false;
        private bool _accessTokenExpired = false;

        public bool IsSignedIn => _isInitialized && !_accessTokenExpired;

        public event EventHandler<string?> EmailChanged;

        public MetaSignInService()
        {
        }

        public async Task InitializeAsync()
        {
            var hasRefreshToken = await GetSecureValueAsync(RefreshTokenKey);
            if (!IsSignedIn && hasRefreshToken != null)
            {
                await SignInAsync();
            }
        }

        public async Task<bool> SignInAsync()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var tokenExpiry = await GetSecureValueAsync(TokenExpiryKey);
            var expiresIn = string.IsNullOrEmpty(tokenExpiry) ? 0 : long.Parse(tokenExpiry);
            var isExpired = now - 10 > expiresIn;
            var hasRefreshToken = await GetSecureValueAsync(RefreshTokenKey);

            if (isExpired && hasRefreshToken != null)
            {
                await RefreshTokenAsync();
            }
            else if (isExpired)
            {
                _isInitialized = false;
                _accessTokenExpired = true;
                return false;
            }

            await OnSignInCompleteAsync();
            return true;
        }

        public async Task SignOutAsync()
        {
            await ClearTokensAsync();
            _isInitialized = false;
            _accessTokenExpired = false;
            OnEmailChanged(null);
        }

        public async Task<string> ListFilesAsync()
        {
            if (!IsSignedIn)
            {
                throw new InvalidOperationException("User must be signed in to list files.");
            }

            var accessToken = await GetSecureValueAsync(AccessTokenKey);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = "https://graph.facebook.com/v19.0/me/photos?limit=20";
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var graphResponse = JsonSerializer.Deserialize<MetaMediaItemResponse>(responseBody);

            var allFiles = new List<string>();
            if (graphResponse.Data != null)
            {
                foreach (var item in graphResponse.Data)
                {
                    var fileInfo = $"File: {item.Name} (Type: {item.Type})";
                    allFiles.Add(fileInfo);
                }
            }

            return string.Join("\n", allFiles);
        }

        protected virtual async Task OnSignInCompleteAsync()
        {
            _isInitialized = true;
            var email = await GetSecureValueAsync(EmailKey);
            OnEmailChanged(email);
        }

        protected virtual void OnEmailChanged(string? email)
        {
            EmailChanged?.Invoke(this, email);
        }

        protected virtual async Task RefreshTokenAsync()
        {
            var clientId = await GetSecureValueAsync("meta_client_id");
            var clientSecret = await GetSecureValueAsync("meta_client_secret");
            var refreshToken = await GetSecureValueAsync(RefreshTokenKey);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Missing required Meta OAuth parameters.");
            }

            var tokenEndpoint = "https://graph.facebook.com/v19.0/oauth/access_token";
            var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Get, tokenEndpoint)
            {
                RequestUri = new Uri($"{tokenEndpoint}?client_id={clientId}&client_secret={clientSecret}&grant_type=refresh_token&refresh_token={refreshToken}")
            };

            var response = await client.SendAsync(tokenRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error refreshing token: {responseBody}");
            }

            var jsonTokens = JsonSerializer.Deserialize<MetaTokenResponse>(responseBody);
            await SetSecureValueAsync(AccessTokenKey, jsonTokens.AccessToken);
            await SetSecureValueAsync(RefreshTokenKey, jsonTokens.RefreshToken);
            await SetSecureValueAsync(EmailKey, await GetSecureValueAsync(EmailKey));
            await SetSecureValueAsync(TokenExpiryKey, (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + jsonTokens.ExpiresIn).ToString());
            _accessTokenExpired = false;
        }

        protected virtual async Task ClearTokensAsync()
        {
            await SetSecureValueAsync(AccessTokenKey, null);
            await SetSecureValueAsync(RefreshTokenKey, null);
            await SetSecureValueAsync(EmailKey, null);
            await SetSecureValueAsync(TokenExpiryKey, null);
        }

        protected virtual async Task SetSecureValueAsync(string key, object value)
        {
            if (value == null)
            {
                _settingsStore.Remove(key);
            }
            else
            {
                _settingsStore[key] = value;
            }
            await Task.CompletedTask;
        }

        public virtual async Task<string?> GetSecureValueAsync(string key)
        {
            return _settingsStore.TryGetValue(key, out var value) ? value?.ToString() : null;
        }

        public void SetTokens(string accessToken, long expiresInSeconds)
        {
            _settingsStore[AccessTokenKey] = accessToken;
            _settingsStore[TokenExpiryKey] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresInSeconds).ToString();
            _isInitialized = true;
            _accessTokenExpired = false;
        }
    }

    public class MetaMediaItemResponse
    {
        [JsonPropertyName("data")]
        public List<MetaMediaItem> Data { get; set; } = new();
    }

    public class MetaMediaItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class MetaTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
    }
}