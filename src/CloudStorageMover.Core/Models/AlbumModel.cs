namespace CloudStorageMover.Core.Models;

public class AlbumModel
{
    public string Name { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public int PhotoCount { get; set; }
    public DateTimeOffset? LastModified { get; set; }
}
