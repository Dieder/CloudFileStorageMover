namespace FileMover.UI.Models;

/// <summary>
/// Represents a file or folder item
/// </summary>
public class FileItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public long Size { get; set; }
    public DateTime Modified { get; set; }
    public string? MimeType { get; set; }
    public bool IsSelected { get; set; }
    
    public string SizeDisplay => FormatSize(Size);
    
    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
