using System.Collections.Generic;

namespace CloudStorageMover.Core.Settings
{
    public class AppSettings
    {
        public SourceFoldersSettings Sources { get; set; } = new();
        public GoogleDriveSettings Providers_GoogleDrive { get; set; } = new();
        public OneDriveSettings Providers_OneDrive { get; set; } = new();
        public MetaSettings Providers_Meta { get; set; } = new();
        public PrimeSettings Providers_Prime { get; set; } = new();
        public OperationsSettings Operations { get; set; } = new();
        public UISettings UI { get; set; } = new();
    }

    public class SourceFoldersSettings
    {
        public string Downloads { get; set; } = "Desktop";
        public string Documents { get; set; } = "Local \\Documents";
        public string Images { get; set; } = "Local \\Pictures";
        public string Videos { get; set; } = "Local \\Videos";
    }

    public class GoogleDriveSettings
    {
        public bool Enabled { get; set; } = false;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class OneDriveSettings
    {
        public bool Enabled { get; set; } = false;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class MetaSettings
    {
        public bool Enabled { get; set; } = false;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class PrimeSettings
    {
        public bool Enabled { get; set; } = false;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class OperationsSettings
    {
        public long MaxFileSize { get; set; } = 52428800;
        public int MaxBatchSize { get; set; } = 1000;
        public bool EnableCompression { get; set; } = true;
        public bool IncludeHiddenFiles { get; set; } = false;
    }

    public class UISettings
    {
        public int WindowWidth { get; set; } = 1200;
        public int WindowHeight { get; set; } = 800;
        public string DefaultTheme { get; set; } = "Light";
        public bool ShowPreviewPane { get; set; } = true;
    }
}