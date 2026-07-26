namespace CloudStorageMover.Core.Services;

public enum FileCategory
{
    Unknown,
    Photo,
    Video,
    Document
}

public class ClassifiedFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public FileCategory Category { get; set; }
    public DateTime? DateTaken { get; set; }
    public string RemotePath { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
}

public static class FileClassificationService
{
    private static readonly HashSet<string> PhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
        ".webp", ".heic", ".heif", ".raw", ".cr2", ".nef", ".arw",
        ".dng", ".rw2", ".orf", ".srw", ".raf"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm",
        ".m4v", ".mpg", ".mpeg", ".3gp", ".ts", ".mts", ".m2ts"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".rtf", ".csv", ".odt", ".ods", ".odp", ".pages",
        ".numbers", ".key", ".md", ".html", ".htm", ".xml", ".json"
    };

    public static FileCategory Classify(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
            return FileCategory.Unknown;

        if (PhotoExtensions.Contains(ext))
            return FileCategory.Photo;
        if (VideoExtensions.Contains(ext))
            return FileCategory.Video;
        if (DocumentExtensions.Contains(ext))
            return FileCategory.Document;

        return FileCategory.Unknown;
    }

    public static string GetSubfolderName(FileCategory category)
    {
        return category switch
        {
            FileCategory.Photo => "Photos",
            FileCategory.Video => "Videos",
            FileCategory.Document => "Documents",
            _ => "Other"
        };
    }

    public static string GetDateSubfolder(DateTime? dateTaken)
    {
        var date = dateTaken ?? DateTime.MinValue;
        return date.ToString("yyyy-MM-dd");
    }

    public static string GetDestinationPath(string rootPath, FileCategory category, DateTime? dateTaken)
    {
        var subfolder = GetSubfolderName(category);

        if (category == FileCategory.Photo)
        {
            var dateFolder = GetDateSubfolder(dateTaken);
            return Path.Combine(rootPath, subfolder, dateFolder);
        }

        return Path.Combine(rootPath, subfolder);
    }
}
