namespace CloudStorageMover.Core.Models;

public class PhotoItem
{
    public string BlobName { get; set; } = string.Empty;
    public string AlbumName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;

    public string SizeText => Size > 1048576
        ? $"{Size / 1048576.0:F1} MB"
        : $"{Size / 1024.0:F0} KB";
}
