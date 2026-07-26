using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Core.Services;

public class DownloadManager
{
    public event Action<DownloadJob>? JobProgressChanged;
    public event Action<DownloadJob>? JobCompleted;
    public event Action<DownloadJob>? JobFailed;
    public event Action<DownloadJob>? JobCancelled;

    public async Task ExecuteAsync(DownloadJob job, IProgress<double>? progress = null)
    {
        if (job.Provider == null)
            throw new InvalidOperationException("No provider assigned to download job.");

        job.State = DownloadJobState.Downloading;
        job.Cts = new CancellationTokenSource();
        NotifyProgress(job);

        try
        {
            var directory = Path.GetDirectoryName(job.LocalPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using var stream = await job.Provider.OpenReadStreamAsync(job.RemotePath, job.Cts.Token);
            if (job.TotalBytes == 0 && stream.CanSeek)
                job.TotalBytes = stream.Length;

            using var fileStream = new FileStream(job.LocalPath, FileMode.Create, FileAccess.Write);
            var buffer = new byte[81920];
            long bytesDownloaded = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, job.Cts.Token)) > 0)
            {
                while (job.State == DownloadJobState.Paused)
                {
                    await Task.Delay(200, job.Cts.Token);
                }

                job.Cts.Token.ThrowIfCancellationRequested();

                await fileStream.WriteAsync(buffer, 0, bytesRead, job.Cts.Token);
                bytesDownloaded += bytesRead;
                job.BytesDownloaded = bytesDownloaded;

                progress?.Report(job.Progress);
                NotifyProgress(job);
            }

            job.State = DownloadJobState.Completed;
            NotifyProgress(job);
            JobCompleted?.Invoke(job);
        }
        catch (OperationCanceledException)
        {
            job.State = DownloadJobState.Cancelled;
            NotifyProgress(job);
            JobCancelled?.Invoke(job);
            CleanupFile(job);
        }
        catch (Exception ex)
        {
            job.State = DownloadJobState.Failed;
            job.ErrorMessage = ex.Message;
            NotifyProgress(job);
            JobFailed?.Invoke(job);
            CleanupFile(job);
        }
        finally
        {
            job.Cts?.Dispose();
            job.Cts = null;
        }
    }

    private static void CleanupFile(DownloadJob job)
    {
        try
        {
            if (File.Exists(job.LocalPath))
                File.Delete(job.LocalPath);
        }
        catch { }
    }

    private void NotifyProgress(DownloadJob job)
    {
        JobProgressChanged?.Invoke(job);
    }
}
