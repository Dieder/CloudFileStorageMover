using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudStorageMover.Core.Services
{
    public class OneDriveSignInService
    {
        private static readonly string[] Scopes = { "Files.ReadWrite.All", "User.Read" };

        private IPublicClientApplication? _pca;
        private AuthenticationResult? _authResult;
        private bool _isInitialized;

        public bool IsSignedIn => _isInitialized && _authResult != null;

        public event EventHandler<string?>? EmailChanged;

        public OneDriveSignInService()
        {
        }

        public async Task InitializeAsync()
        {
            var clientId = await GetSecureValueAsync("onedrive_client_id");
            if (string.IsNullOrEmpty(clientId))
            {
                return;
            }

            _pca = PublicClientApplicationBuilder.Create(clientId)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();

            var accounts = await _pca.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    _authResult = await _pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                    _isInitialized = true;
                    OnEmailChanged(_authResult.Account.Username);
                }
                catch (MsalUiRequiredException)
                {
                }
            }
        }

        public async Task SignInAsync()
        {
            if (_pca == null)
            {
                var clientId = await GetSecureValueAsync("onedrive_client_id");
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new InvalidOperationException("OneDrive client ID not configured.");
                }

                _pca = PublicClientApplicationBuilder.Create(clientId)
                    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                    .Build();
            }

            _authResult = await _pca.AcquireTokenInteractive(Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            _isInitialized = true;
            OnEmailChanged(_authResult.Account.Username);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_pca == null || _authResult == null)
            {
                throw new InvalidOperationException("User must be signed in first.");
            }

            try
            {
                var accounts = await _pca.GetAccountsAsync();
                _authResult = await _pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
                return _authResult.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                _authResult = await _pca.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
                return _authResult.AccessToken;
            }
        }

        public async Task SignOutAsync()
        {
            if (_pca != null)
            {
                var accounts = await _pca.GetAccountsAsync();
                foreach (var account in accounts)
                {
                    await _pca.RemoveAsync(account);
                }
            }

            _authResult = null;
            _isInitialized = false;
            OnEmailChanged(null);
        }

        protected virtual void OnEmailChanged(string? email)
        {
            EmailChanged?.Invoke(this, email);
        }

        private async Task<string?> GetSecureValueAsync(string key)
        {
            await Task.CompletedTask;
            return null;
        }
    }
}
