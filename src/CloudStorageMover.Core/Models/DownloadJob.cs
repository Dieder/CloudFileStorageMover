using CloudStorageMover.Core.Interfaces;

namespace CloudStorageMover.Core.Models;

public class DownloadJob
{
    public Guid Id { get; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string RemotePath { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long BytesDownloaded { get; set; }
    public DownloadJobState State { get; set; } = DownloadJobState.Queued;
    public string? ErrorMessage { get; set; }
    public IStorageProviderService? Provider { get; set; }
    public CancellationTokenSource? Cts { get; set; }

    public double Progress => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes : 0;

    public void Pause()
    {
        if (State == DownloadJobState.Downloading)
            State = DownloadJobState.Paused;
    }

    public void Resume()
    {
        if (State == DownloadJobState.Paused)
            State = DownloadJobState.Downloading;
    }

    public void Cancel()
    {
        if (State is DownloadJobState.Queued or DownloadJobState.Downloading or DownloadJobState.Paused)
        {
            Cts?.Cancel();
            State = DownloadJobState.Cancelled;
        }
    }
}
