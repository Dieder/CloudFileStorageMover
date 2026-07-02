using FileMover.UI.Models;

namespace FileMover.UI.Services;

public class TransferSessionService
{
    public StorageLocation? SourceLocation { get; set; }
    public StorageLocation? DestinationLocation { get; set; }
    public List<FileItem> SelectedFiles { get; set; } = new();
    public Models.TransferMode SelectedTransferMode { get; set; } = Models.TransferMode.CloudToCloud;
    public bool IsServerMode { get; set; }
    public string? ServerAddress { get; set; }
    public int ServerPort { get; set; } = 5000;

    public TransferSession CreateSession()
    {
        return new TransferSession
        {
            SourceLocation = SourceLocation,
            DestinationLocation = DestinationLocation,
            SelectedItems = new List<FileItem>(SelectedFiles),
            Mode = SelectedTransferMode,
            ServerAddress = ServerAddress,
            ServerPort = ServerPort,
            SessionName = $"{SourceLocation?.DisplayName ?? "Unknown"} -> {DestinationLocation?.DisplayName ?? "Unknown"}"
        };
    }

    public void Reset()
    {
        SourceLocation = null;
        DestinationLocation = null;
        SelectedFiles.Clear();
        SelectedTransferMode = Models.TransferMode.CloudToCloud;
        IsServerMode = false;
        ServerAddress = null;
        ServerPort = 5000;
    }
}
