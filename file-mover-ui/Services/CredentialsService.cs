using System.Text.Json;

namespace FileMover.UI.Services;

public class CredentialsService : ICredentialsService
{
    private const string KeyPrefix = "credentials_";

    public async Task SaveCredentialsAsync(string providerId, Dictionary<string, string> credentials)
    {
        var json = JsonSerializer.Serialize(credentials);
        try
        {
            await SecureStorage.SetAsync(KeyPrefix + providerId, json);
        }
        catch
        {
            await File.WriteAllTextAsync(GetFallbackPath(providerId), json);
        }
    }

    public async Task<Dictionary<string, string>?> GetCredentialsAsync(string providerId)
    {
        try
        {
            var json = await SecureStorage.GetAsync(KeyPrefix + providerId);
            if (json != null)
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch
        {
        }

        var fallbackPath = GetFallbackPath(providerId);
        if (File.Exists(fallbackPath))
        {
            var json = await File.ReadAllTextAsync(fallbackPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        return null;
    }

    public async Task ClearCredentialsAsync(string providerId)
    {
        SecureStorage.Remove(KeyPrefix + providerId);
        var fallbackPath = GetFallbackPath(providerId);
        if (File.Exists(fallbackPath))
            File.Delete(fallbackPath);
        await Task.CompletedTask;
    }

    public async Task<bool> IsConfiguredAsync(string providerId)
    {
        var credentials = await GetCredentialsAsync(providerId);
        return credentials != null && credentials.Count > 0;
    }

    private static string GetFallbackPath(string providerId)
    {
        var dir = Path.Combine(FileSystem.AppDataDirectory, ".credentials");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{providerId}.json");
    }
}
