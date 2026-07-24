namespace CloudStorageMover.Core.Services;

public class DestinationSettingsService
{
    private readonly string _settingsPath;

    public string DestinationPath { get; private set; } = GetDefaultPath();

    public string GooglePhotosDownloadPath { get; private set; } = GetDefaultGooglePhotosPath();

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

            var data = new Dictionary<string, string>
            {
                ["destinationPath"] = DestinationPath,
                ["googlePhotosDownloadPath"] = GooglePhotosDownloadPath
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
