namespace FileMover.UI.Models;

/// <summary>
/// Represents an active transfer session
/// </summary>
public class TransferSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionName { get; set; } = string.Empty;
    public TransferMode Mode { get; set; }
    public StorageLocation? SourceLocation { get; set; }
    public StorageLocation? DestinationLocation { get; set; }
    public List<FileItem> SelectedItems { get; set; } = new();
    public string? ConnectionToken { get; set; }
    public string? ServerAddress { get; set; }
    public int ServerPort { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
