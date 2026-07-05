namespace CloudStorageMover.Core.Models;

public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDirectory { get; set; }
    public string? IconPath { get; set; }
    public StorageProviderType ProviderType { get; set; }
}
