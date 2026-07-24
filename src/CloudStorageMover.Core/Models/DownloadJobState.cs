namespace CloudStorageMover.Core.Models;

public enum DownloadJobState
{
    Queued,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled
}
