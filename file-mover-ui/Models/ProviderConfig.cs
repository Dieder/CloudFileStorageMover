using System.Text.Json.Serialization;

namespace FileMover.UI.Models;

public class ProviderConfig
{
    [JsonPropertyName("providerId")]
    public string ProviderId { get; set; } = string.Empty;

    [JsonPropertyName("providerName")]
    public string ProviderName { get; set; } = string.Empty;

    [JsonPropertyName("clientSecretsPath")]
    public string? ClientSecretsPath { get; set; }

    [JsonPropertyName("applicationName")]
    public string? ApplicationName { get; set; }

    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }

    [JsonPropertyName("baseDirectory")]
    public string? BaseDirectory { get; set; }

    [JsonPropertyName("allowedPaths")]
    public List<string>? AllowedPaths { get; set; }

    [JsonPropertyName("apiBaseUrl")]
    public string? ApiBaseUrl { get; set; }

    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("apiSecret")]
    public string? ApiSecret { get; set; }
}
