namespace FileMover.UI.Models;

/// <summary>
/// Enumeration of transfer modes
/// </summary>
public enum TransferMode
{
    CloudToCloud,    // Transfer between cloud storage providers
    CloudToLocal,    // Transfer from cloud to local device
    LocalToCloud,    // Transfer from local device to cloud
    WiFiDirect       // WiFi direct transfer between devices
}
