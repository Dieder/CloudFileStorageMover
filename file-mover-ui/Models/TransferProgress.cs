namespace FileMover.UI.Models;

/// <summary>
/// Represents the progress of an active transfer
/// </summary>
public class TransferProgress
{
    public string SessionId { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public long TotalBytes { get; set; }
    public long TransferredBytes { get; set; }
    public double ProgressPercentage => TotalBytes > 0 ? (TransferredBytes / (double)TotalBytes) * 100 : 0;
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public string? CurrentFile { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Enumeration of transfer statuses
/// </summary>
public enum TransferStatus
{
    Pending,
    InProgress,
    Paused,
    Completed,
    Failed,
    Cancelled
}
