using System.Text.Json;
using FileMover.UI.Models;

namespace FileMover.UI.Services;

public class ProviderConfigLoader
{
    private readonly string _configDirectory;

    public ProviderConfigLoader(string? configDirectory = null)
    {
        _configDirectory = configDirectory
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "providers");
    }

    public async Task<ProviderConfig?> LoadAsync(string providerId)
    {
        var path = Path.Combine(_configDirectory, $"{providerId}.json");
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ProviderConfig>(json);
    }

    public async Task<List<ProviderConfig>> LoadAllAsync()
    {
        if (!Directory.Exists(_configDirectory))
            return [];

        var configs = new List<ProviderConfig>();
        foreach (var file in Directory.EnumerateFiles(_configDirectory, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var config = JsonSerializer.Deserialize<ProviderConfig>(json);
            if (config is not null)
                configs.Add(config);
        }
        return configs;
    }
}
