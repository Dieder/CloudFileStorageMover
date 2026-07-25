#if WINDOWS
using MediaDevices;

namespace CloudStorageMover.App.Services;

public class CameraImportService
{
    private MediaDevice? _device;
    private string? _drivePath;

    public bool IsConnected => _device?.IsConnected == true || !string.IsNullOrEmpty(_drivePath);

    public List<CameraDevice> GetDevices()
    {
        var result = new List<CameraDevice>();

        // MTP/PTP devices
        try
        {
            foreach (var d in MediaDevice.GetDevices())
            {
                result.Add(new CameraDevice
                {
                    DeviceId = d.DeviceId,
                    FriendlyName = d.FriendlyName,
                    Manufacturer = d.Manufacturer ?? string.Empty,
                    Model = d.Model ?? string.Empty,
                    ConnectionType = CameraConnectionType.Mtp
                });
            }
        }
        catch
        {
            // MTP enumeration failed
        }

        // Drive-based cameras (mass storage)
        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Removable || !drive.IsReady)
                    continue;

                var dcimPath = Path.Combine(drive.RootDirectory.FullName, "DCIM");
                if (Directory.Exists(dcimPath))
                {
                    var label = string.IsNullOrEmpty(drive.VolumeLabel)
                        ? $"Removable ({drive.Name})"
                        : $"{drive.VolumeLabel} ({drive.Name})";

                    result.Add(new CameraDevice
                    {
                        DeviceId = $"drive:{drive.RootDirectory.FullName}",
                        FriendlyName = label,
                        Manufacturer = "Drive",
                        Model = drive.VolumeLabel ?? "Removable",
                        ConnectionType = CameraConnectionType.Drive
                    });
                }
            }
        }
        catch
        {
            // Drive enumeration failed
        }

        return result.Where(d => !string.IsNullOrEmpty(d.FriendlyName)).ToList();
    }

    public bool Connect(string deviceId)
    {
        Disconnect();

        if (deviceId.StartsWith("drive:"))
        {
            _drivePath = deviceId.Substring("drive:".Length);
            return Directory.Exists(Path.Combine(_drivePath, "DCIM"));
        }

        var devices = MediaDevice.GetDevices();
        _device = devices.FirstOrDefault(d => d.DeviceId == deviceId);

        if (_device == null)
            return false;

        _device.Connect();
        return _device.IsConnected;
    }

    public void Disconnect()
    {
        if (_device?.IsConnected == true)
            _device.Disconnect();
        _device = null;
        _drivePath = null;
    }

    public List<CameraPhoto> GetAllPhotos()
    {
        if (!string.IsNullOrEmpty(_drivePath))
            return GetAllPhotosFromDrive(_drivePath);

        return GetAllPhotosFromMtp();
    }

    public HashSet<string> GetExistingFiles(string outputFolder)
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(outputFolder))
            return existing;

        try
        {
            foreach (var file in Directory.GetFiles(outputFolder))
            {
                existing.Add(Path.GetFileName(file));
            }
        }
        catch { }

        return existing;
    }

    public int CountPhotosToImport(List<CameraPhoto> photos, HashSet<string> existingFiles)
    {
        return photos.Count(p => !existingFiles.Contains(p.FileName));
    }

    private List<CameraPhoto> GetAllPhotosFromDrive(string drivePath)
    {
        var result = new List<CameraPhoto>();
        var dcimPath = Path.Combine(drivePath, "DCIM");

        if (!Directory.Exists(dcimPath))
            return result;

        try
        {
            foreach (var folder in Directory.GetDirectories(dcimPath))
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    var name = Path.GetFileName(file);
                    if (IsPhotoFile(name))
                    {
                        var info = new FileInfo(file);
                        result.Add(new CameraPhoto
                        {
                            FileName = name,
                            FilePath = file,
                            Size = info.Length,
                            DateTaken = info.LastWriteTime
                        });
                    }
                }
            }
        }
        catch { }

        return result;
    }

    private List<CameraPhoto> GetAllPhotosFromMtp()
    {
        var result = new List<CameraPhoto>();

        if (_device?.IsConnected != true)
            return result;

        try
        {
            var dcim = _device.GetDirectoryInfo(@"\DCIM");
            if (dcim == null)
                return result;

            foreach (var folder in dcim.EnumerateDirectories())
            {
                foreach (var file in folder.EnumerateFiles())
                {
                    var name = file.Name;
                    if (name != null && IsPhotoFile(name))
                    {
                        var lastWrite = file.LastWriteTime ?? DateTime.MinValue;
                        result.Add(new CameraPhoto
                        {
                            FileName = name,
                            FilePath = file.FullName,
                            Size = (long)file.Length,
                            DateTaken = lastWrite
                        });
                    }
                }
            }
        }
        catch { }

        return result;
    }

    public bool DownloadPhoto(CameraPhoto photo, string outputFolder, bool deleteAfterImport, HashSet<string> existingFiles)
    {
        try
        {
            if (existingFiles.Contains(photo.FileName))
            {
                if (deleteAfterImport)
                    DeletePhoto(photo.FilePath);
                return true;
            }

            var targetPath = GetUniqueFilePath(outputFolder, photo.FileName);

            if (!string.IsNullOrEmpty(_drivePath))
            {
                File.Copy(photo.FilePath, targetPath, true);
            }
            else if (_device?.IsConnected == true)
            {
                _device.DownloadFile(photo.FilePath, targetPath);
            }
            else
            {
                return false;
            }

            existingFiles.Add(Path.GetFileName(targetPath));

            if (deleteAfterImport)
                DeletePhoto(photo.FilePath);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void DeletePhoto(string filePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(_drivePath))
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            else if (_device?.IsConnected == true)
            {
                _device.DeleteFile(filePath);
            }
        }
        catch { }
    }

    private static bool IsPhotoFile(string name)
    {
        return name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".rw2", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".cr2", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".nef", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".arw", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".dng", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);
    }

    private string GetUniqueFilePath(string folder, string fileName)
    {
        var path = Path.Combine(folder, fileName);
        if (!File.Exists(path))
            return path;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var counter = 1;

        do
        {
            path = Path.Combine(folder, $"{nameWithoutExt}({counter}){ext}");
            counter++;
        } while (File.Exists(path));

        return path;
    }
}

public enum CameraConnectionType
{
    Mtp,
    Drive
}

public class CameraDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public CameraConnectionType ConnectionType { get; set; }

    public string DisplayName => string.IsNullOrEmpty(Model)
        ? FriendlyName
        : $"{Manufacturer} {Model}".Trim();

    public string ConnectionLabel => ConnectionType == CameraConnectionType.Mtp
        ? "MTP/PTP"
        : "USB Drive";
}

public class CameraPhoto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime DateTaken { get; set; }
}

public class CameraImportResult
{
    public int TotalFound { get; set; }
    public int Imported { get; set; }
    public int SkippedDuplicate { get; set; }
    public int Failed { get; set; }
    public string Message { get; set; } = string.Empty;
}
#endif
