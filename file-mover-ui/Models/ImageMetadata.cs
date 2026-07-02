namespace FileMover.UI.Models;

/// <summary>
/// Represents EXIF metadata for images
/// </summary>
public class ImageMetadata
{
    public string? CameraModel { get; set; }
    public DateTime? DateTaken { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? FocalLength { get; set; }
    public string? FNumber { get; set; }
    public string? ExposureTime { get; set; }
    public string? ISO { get; set; }
    public string? Flash { get; set; }
}
