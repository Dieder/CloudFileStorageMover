namespace CloudStorageMover.Core.Services;

public class PCloudSignInService
{
    private const string TokenKey = "PCloudAccessToken";

    public bool IsSignedIn => !string.IsNullOrEmpty(AccessToken);

    public string? AccessToken { get; private set; }

    public void SignIn(string accessToken)
    {
        AccessToken = accessToken;
    }

    public void SignOut()
    {
        AccessToken = null;
    }

    public async Task<(bool Success, string? Token)> AuthenticateAsync(string username, string password)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.pcloud.com/") };
            var form = new Dictionary<string, string>
            {
                ["username"] = username,
                ["password"] = password,
                ["getauth"] = "1"
            };
            using var content = new FormUrlEncodedContent(form);
            var response = await client.PostAsync("auth2", content);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("result", out var result) && result.GetInt32() == 0)
            {
                var token = root.GetProperty("auth").GetString();
                AccessToken = token;
                return (true, token);
            }

            return (false, null);
        }
        catch
        {
            return (false, null);
        }
    }
}
