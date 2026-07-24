namespace CloudStorageMover.Core.Services
{
    public class GoogleDriveSignInService
    {
        private const string AccessTokenKey = "google_access_token";
        private const string RefreshTokenKey = "google_refresh_token";
        private const string EmailKey = "google_email";
        private const string TokenExpiryKey = "google_token_expiry";

        private readonly Dictionary<string, object> _settingsStore;
        private readonly AppSettingsService _settings;

        private bool _isInitialized = false;
        private bool _accessTokenExpired = false;

        public GoogleDriveSignInService(AppSettingsService settings)
        {
            _settingsStore = new Dictionary<string, object>();
            _settings = settings;

            if (!string.IsNullOrEmpty(_settings.GoogleDriveClientId))
                _settingsStore["google_client_id"] = _settings.GoogleDriveClientId;
            if (!string.IsNullOrEmpty(_settings.GoogleDriveSecret))
                _settingsStore["google_client_secret"] = _settings.GoogleDriveSecret;
        }

        public bool IsSignedIn => _isInitialized && !_accessTokenExpired;

        public event EventHandler<string?> EmailChanged;

        public async Task InitializeAsync()
        {
            var hasRefreshToken = await GetSecureValueAsync(RefreshTokenKey);
            if (!IsSignedIn && hasRefreshToken != null)
            {
                await SignInAsync();
            }
        }

        public async Task SignInAsync()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var tokenExpiry = await GetSecureValueAsync(TokenExpiryKey);
            var expiresIn = tokenExpiry != null ? long.Parse(tokenExpiry) : 0;
            var isExpired = now - 10 > expiresIn;
            var hasRefreshToken = await GetSecureValueAsync(RefreshTokenKey);

            if (isExpired && hasRefreshToken != null)
            {
                await RefreshTokenAsync();
            }
            else if (isExpired)
            {
                throw new InvalidOperationException("No refresh token available for authentication.");
            }

            await OnSignInCompleteAsync();
        }

        public async Task SignOutAsync()
        {
            await ClearTokensAsync();
            _isInitialized = false;
            _accessTokenExpired = false;
            OnEmailChanged(null);
        }

        public async Task<string> ListFilesAsync(string folderId = null)
        {
            if (!IsSignedIn)
            {
                throw new InvalidOperationException("User must be signed in to list files.");
            }

            var accessToken = await GetSecureValueAsync(AccessTokenKey);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = "https://www.googleapis.com/drive/v3/files";
            if (!string.IsNullOrEmpty(folderId))
            {
                requestUrl += $"?q=parents='{folderId}'";
            }

            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
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
            var clientId = _settings.GoogleDriveClientId;
            var clientSecret = _settings.GoogleDriveSecret;
            var refreshToken = await GetSecureValueAsync(RefreshTokenKey);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Missing required Google OAuth parameters.");
            }

            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var client = new HttpClient();
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };

            if (!string.IsNullOrEmpty(clientSecret))
            {
                formData.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            }

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(formData)
            };

            var response = await client.SendAsync(tokenRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error refreshing token: {responseBody}");
            }

            var jsonTokens = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            var accessToken = jsonTokens["access_token"].ToString();
            var refreshTokenNew = jsonTokens.ContainsKey("refresh_token") ? jsonTokens["refresh_token"].ToString() : refreshToken;
            var expiresIn = int.Parse(jsonTokens["expires_in"].ToString());

            await SetSecureValueAsync(AccessTokenKey, accessToken);
            await SetSecureValueAsync(RefreshTokenKey, refreshTokenNew);
            await SetSecureValueAsync(EmailKey, await GetSecureValueAsync(EmailKey));
            await SetSecureValueAsync(TokenExpiryKey, (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn).ToString());
            _accessTokenExpired = false;
        }

        public void SetTokens(string accessToken, string? refreshToken, long expiresIn)
        {
            _settingsStore[AccessTokenKey] = accessToken;
            if (!string.IsNullOrEmpty(refreshToken))
                _settingsStore[RefreshTokenKey] = refreshToken;
            _settingsStore[TokenExpiryKey] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn).ToString();
            _isInitialized = true;
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
    }
}