using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudStorageMover.Core.Services
{
    public class PrimeSignInService
    {
        private const string AccessTokenKey = "prime_access_token";
        private const string RefreshTokenKey = "prime_refresh_token";
        private const string EmailKey = "prime_email";
        private const string TokenExpiryKey = "prime_token_expiry";

        private readonly Dictionary<string, object> _settingsStore = new();
        private bool _isInitialized;
        private bool _accessTokenExpired;

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
            var expiresIn = string.IsNullOrEmpty(tokenExpiry) ? 0 : long.Parse(tokenExpiry);
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
            var refreshToken = await GetSecureValueAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Missing refresh token for Prime authentication.");
            }

            await SetSecureValueAsync(AccessTokenKey, refreshToken);
            _accessTokenExpired = false;
        }

        protected virtual async Task ClearTokensAsync()
        {
            await SetSecureValueAsync(AccessTokenKey, null);
            await SetSecureValueAsync(RefreshTokenKey, null);
            await SetSecureValueAsync(EmailKey, null);
            await SetSecureValueAsync(TokenExpiryKey, null);
        }

        protected virtual async Task SetSecureValueAsync(string key, object? value)
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

        public void SetTokens(string accessToken, string? refreshToken, long expiresInSeconds)
        {
            _settingsStore[AccessTokenKey] = accessToken;
            if (refreshToken != null)
                _settingsStore[RefreshTokenKey] = refreshToken;
            _settingsStore[TokenExpiryKey] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresInSeconds).ToString();
            _isInitialized = true;
            _accessTokenExpired = false;
        }
    }
}
