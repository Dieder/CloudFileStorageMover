namespace FileMover.UI.Models;

/// <summary>
/// Represents a cloud storage provider (AWS S3, Azure Blob, Google Cloud Storage, etc.)
/// </summary>
public class StorageProvider
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // s3, azure, gcs, etc.
    public string? IconUrl { get; set; }
    public bool IsConfigured { get; set; }
}
