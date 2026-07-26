namespace CloudStorageMover.Core.Services;

public class DestinationSettingsService
{
    private readonly string _settingsPath;

    public string DestinationPath { get; private set; } = GetDefaultPath();

    public string GooglePhotosDownloadPath { get; private set; } = GetDefaultGooglePhotosPath();

    public string PhotosSubfolder { get; private set; } = "Photos";

    public string VideosSubfolder { get; private set; } = "Videos";

    public string DocumentsSubfolder { get; private set; } = "Documents";

    public bool OrganizePhotosByDate { get; private set; } = true;

    public string MegaKey { get; private set; } = string.Empty;

    public string AzureConnectionString { get; private set; } = string.Empty;

    public event Action<string>? DestinationPathChanged;

    public event Action<string>? GooglePhotosDownloadPathChanged;

    public DestinationSettingsService(string? settingsDirectory = null)
    {
        _settingsPath = string.IsNullOrEmpty(settingsDirectory)
            ? string.Empty
            : Path.Combine(settingsDirectory, "destination_settings.json");
    }

    public void SetDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = GetDefaultPath();

        DestinationPath = path;
        DestinationPathChanged?.Invoke(path);
        Save();
    }

    public void SetGooglePhotosDownloadPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = GetDefaultGooglePhotosPath();

        GooglePhotosDownloadPath = path;
        GooglePhotosDownloadPathChanged?.Invoke(path);
        Save();
    }

    public void SetPhotosSubfolder(string subfolder)
    {
        PhotosSubfolder = string.IsNullOrWhiteSpace(subfolder) ? "Photos" : subfolder.Trim();
        Save();
    }

    public void SetVideosSubfolder(string subfolder)
    {
        VideosSubfolder = string.IsNullOrWhiteSpace(subfolder) ? "Videos" : subfolder.Trim();
        Save();
    }

    public void SetDocumentsSubfolder(string subfolder)
    {
        DocumentsSubfolder = string.IsNullOrWhiteSpace(subfolder) ? "Documents" : subfolder.Trim();
        Save();
    }

    public void SetOrganizePhotosByDate(bool organize)
    {
        OrganizePhotosByDate = organize;
        Save();
    }

    public void SetMegaKey(string key)
    {
        MegaKey = key?.Trim() ?? string.Empty;
        Save();
    }

    public void SetAzureConnectionString(string connectionString)
    {
        AzureConnectionString = connectionString?.Trim() ?? string.Empty;
        Save();
    }

    public string GetTransferDestination(FileCategory category, DateTime? dateTaken = null)
    {
        var subfolder = category switch
        {
            FileCategory.Photo => PhotosSubfolder,
            FileCategory.Video => VideosSubfolder,
            FileCategory.Document => DocumentsSubfolder,
            _ => "Other"
        };

        if (category == FileCategory.Photo && OrganizePhotosByDate && dateTaken.HasValue)
        {
            var dateFolder = dateTaken.Value.ToString("yyyy-MM-dd");
            return Path.Combine(DestinationPath, subfolder, dateFolder);
        }

        return Path.Combine(DestinationPath, subfolder);
    }

    public static List<RemovableDrive> GetRemovableDrives()
    {
        var drives = new List<RemovableDrive>();

        try
        {
            foreach (var driveInfo in DriveInfo.GetDrives())
            {
                if (driveInfo.DriveType == DriveType.Removable && driveInfo.IsReady)
                {
                    drives.Add(new RemovableDrive
                    {
                        Name = driveInfo.Name,
                        VolumeLabel = driveInfo.VolumeLabel,
                        FreeSpace = driveInfo.AvailableFreeSpace,
                        TotalSize = driveInfo.TotalSize,
                        DisplayLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel)
                            ? $"Removable ({driveInfo.Name})"
                            : $"{driveInfo.VolumeLabel} ({driveInfo.Name})"
                    });
                }
            }
        }
        catch
        {
            // Drive enumeration failed
        }

        return drives;
    }

    public bool HasValidDestination()
    {
        try
        {
            return Directory.Exists(DestinationPath);
        }
        catch
        {
            return false;
        }
    }

    public bool HasValidGooglePhotosDestination()
    {
        try
        {
            return Directory.Exists(GooglePhotosDownloadPath);
        }
        catch
        {
            return false;
        }
    }

    public long GetFreeSpace()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(DestinationPath) ?? DestinationPath);
            return driveInfo.AvailableFreeSpace;
        }
        catch
        {
            return long.MaxValue;
        }
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(_settingsPath) || !File.Exists(_settingsPath))
            return;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("destinationPath", out var dp))
                DestinationPath = dp.GetString() ?? GetDefaultPath();

            if (root.TryGetProperty("googlePhotosDownloadPath", out var gp))
                GooglePhotosDownloadPath = gp.GetString() ?? GetDefaultGooglePhotosPath();

            if (root.TryGetProperty("photosSubfolder", out var ps))
                PhotosSubfolder = ps.GetString() ?? "Photos";

            if (root.TryGetProperty("videosSubfolder", out var vs))
                VideosSubfolder = vs.GetString() ?? "Videos";

            if (root.TryGetProperty("documentsSubfolder", out var ds))
                DocumentsSubfolder = ds.GetString() ?? "Documents";

            if (root.TryGetProperty("organizePhotosByDate", out var ob))
                OrganizePhotosByDate = ob.GetBoolean();

            if (root.TryGetProperty("megaKey", out var mk))
                MegaKey = mk.GetString() ?? string.Empty;

            if (root.TryGetProperty("azureConnectionString", out var acs))
                AzureConnectionString = acs.GetString() ?? string.Empty;
        }
        catch
        {
            // Use defaults
        }
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(_settingsPath))
            return;

        try
        {
            var dir = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var data = new Dictionary<string, object>
            {
                ["destinationPath"] = DestinationPath,
                ["googlePhotosDownloadPath"] = GooglePhotosDownloadPath,
                ["photosSubfolder"] = PhotosSubfolder,
                ["videosSubfolder"] = VideosSubfolder,
                ["documentsSubfolder"] = DocumentsSubfolder,
                ["organizePhotosByDate"] = OrganizePhotosByDate,
                ["megaKey"] = MegaKey,
                ["azureConnectionString"] = AzureConnectionString
            };
            var json = System.Text.Json.JsonSerializer.Serialize(data,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail
        }
    }

    private static string GetDefaultPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private static string GetDefaultGooglePhotosPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Google Photos");
    }
}

public class RemovableDrive
{
    public string Name { get; set; } = string.Empty;
    public string VolumeLabel { get; set; } = string.Empty;
    public long FreeSpace { get; set; }
    public long TotalSize { get; set; }
    public string DisplayLabel { get; set; } = string.Empty;

    public string FreeSpaceText => $"{FreeSpace / (1024.0 * 1024 * 1024):F1} GB free";
    public string TotalSizeText => $"{TotalSize / (1024.0 * 1024 * 1024):F1} GB";
}
