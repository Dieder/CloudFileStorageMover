using System.Text.Json;

namespace CloudStorageMover.Core.Services;

public class AppSettingsService
{
    private Dictionary<string, JsonElement> _settings = new();

    public void Load(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            _settings = new Dictionary<string, JsonElement>();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                _settings[prop.Name] = prop.Value.Clone();
            }
        }
        catch
        {
            _settings = new Dictionary<string, JsonElement>();
        }
    }

    public string? GetValue(string section, string key)
    {
        if (_settings.TryGetValue(section, out var sectionEl) &&
            sectionEl.TryGetProperty(key, out var value))
        {
            return value.GetString();
        }
        return null;
    }

    public string? GoogleDriveClientId => GetValue("googleauthsettings", "clientId");
    public string? GoogleDriveSecret => GetValue("googleauthsettings", "secret");
    public string? GooglePhotosClientId => GetValue("googleauthsettings", "clientId");
    public string? GooglePhotosSecret => GetValue("googleauthsettings", "secret");
    public string? MegaKey => GetValue("megasettings", "key");
}
