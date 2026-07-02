namespace FileMover.UI.Models;

/// <summary>
/// Represents a specific location within a storage provider (bucket/folder path)
/// </summary>
public class StorageLocation
{
    public string Id { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}
