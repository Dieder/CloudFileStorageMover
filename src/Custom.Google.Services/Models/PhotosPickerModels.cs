using System.Text.Json.Serialization;

namespace Custom.Google.Services.Models;

public sealed class PhotosPickerSession
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("pickerUri")]
    public string? PickerUri { get; set; }

    [JsonPropertyName("mediaItemsSet")]
    public bool? MediaItemsSet { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class MediaItemsResponse
{
    [JsonPropertyName("mediaItems")]
    public List<MediaItem>? MediaItems { get; set; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public sealed class MediaItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}
