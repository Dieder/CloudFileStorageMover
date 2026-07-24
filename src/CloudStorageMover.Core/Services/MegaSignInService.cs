using System.Text.Json;
using CG.Web.MegaApiClient;

namespace CloudStorageMover.Core.Services;

public class MegaSignInService
{
    private const string AuthInfosKey = "mega_auth_infos";
    private const string EmailKey = "mega_email";

    private readonly Dictionary<string, object> _settingsStore = new();
    private readonly MegaApiClient _client = new();
    private bool _isInitialized;

    public bool IsSignedIn => _isInitialized && _client.IsLoggedIn;

    public event EventHandler<string?> EmailChanged;

    public MegaSignInService()
    {
    }

    public bool TryRestoreFromKey(string key, string? email = null)
    {
        if (_client.IsLoggedIn)
            return true;

        try
        {
            var authInfos = JsonSerializer.Deserialize<MegaApiClient.AuthInfos>(key);
            if (authInfos != null)
            {
                _client.Login(authInfos);
                _isInitialized = true;
                OnEmailChanged(email);
                return true;
            }
        }
        catch { }

        try
        {
            var token = JsonSerializer.Deserialize<MegaApiClient.LogonSessionToken>(key);
            if (token != null)
            {
                _client.Login(token);
                _isInitialized = true;
                OnEmailChanged(email);
                return true;
            }
        }
        catch { }

        return false;
    }

    public async Task InitializeAsync()
    {
        var serialized = await GetSecureValueAsync(AuthInfosKey);
        if (!string.IsNullOrEmpty(serialized))
        {
            try
            {
                var authInfos = JsonSerializer.Deserialize<MegaApiClient.AuthInfos>(serialized);
                if (authInfos != null)
                {
                    _client.Login(authInfos);
                    _isInitialized = true;
                    var email = await GetSecureValueAsync(EmailKey);
                    OnEmailChanged(email);
                }
            }
            catch
            {
                _isInitialized = false;
            }
        }
    }

    public async Task SignInAsync(string email, string password)
    {
        if (_client.IsLoggedIn)
            _client.Logout();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var authInfos = await Task.Run(() => _client.GenerateAuthInfos(email, password), cts.Token);
        _client.Login(authInfos);

        var serialized = JsonSerializer.Serialize(authInfos);
        await SetSecureValueAsync(AuthInfosKey, serialized);
        await SetSecureValueAsync(EmailKey, email);

        _isInitialized = true;
        OnEmailChanged(email);
    }

    public async Task SignOutAsync()
    {
        if (_client.IsLoggedIn)
            await _client.LogoutAsync();

        await ClearTokensAsync();
        _isInitialized = false;
        OnEmailChanged(null);
    }

    public MegaApiClient GetClient()
    {
        if (!_isInitialized || !_client.IsLoggedIn)
            throw new InvalidOperationException("MEGA client is not signed in.");
        return _client;
    }

    protected virtual void OnEmailChanged(string? email)
    {
        EmailChanged?.Invoke(this, email);
    }

    protected virtual async Task ClearTokensAsync()
    {
        await SetSecureValueAsync(AuthInfosKey, null);
        await SetSecureValueAsync(EmailKey, null);
    }

    protected virtual async Task SetSecureValueAsync(string key, object? value)
    {
        if (value == null)
            _settingsStore.Remove(key);
        else
            _settingsStore[key] = value;
        await Task.CompletedTask;
    }

    public virtual async Task<string?> GetSecureValueAsync(string key)
    {
        return _settingsStore.TryGetValue(key, out var value) ? value?.ToString() : null;
    }
}
