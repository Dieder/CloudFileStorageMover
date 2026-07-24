using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class DownloadQueue
{
    private readonly DownloadManager _manager;
    private readonly ILogger<DownloadQueue> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<DownloadJob> _pending = new();
    private int _activeCount;

    public int MaxConcurrent { get; }

    public IReadOnlyList<DownloadJob> Jobs => _jobs.AsReadOnly();
    private readonly List<DownloadJob> _jobs = new();

    public event Action<DownloadJob>? JobEnqueued;
    public event Action<DownloadJob>? JobStarted;
    public event Action<DownloadJob>? JobProgressChanged;
    public event Action<DownloadJob>? JobCompleted;
    public event Action<DownloadJob>? JobFailed;
    public event Action<DownloadJob>? JobCancelled;

    public DownloadQueue(DownloadManager manager, ILogger<DownloadQueue> logger, int maxConcurrent = 3)
    {
        _manager = manager;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrent);
        MaxConcurrent = maxConcurrent;

        _manager.JobCompleted += OnJobCompleted;
        _manager.JobFailed += OnJobFailed;
        _manager.JobCancelled += OnJobCancelled;
        _manager.JobProgressChanged += OnJobProgressChanged;
    }

    public void Enqueue(DownloadJob job)
    {
        job.State = DownloadJobState.Queued;
        _jobs.Add(job);
        _pending.Enqueue(job);
        JobEnqueued?.Invoke(job);
        _logger.LogInformation("Enqueued download: {FileName}", job.FileName);
        _ = ProcessQueueAsync();
    }

    public void EnqueueRange(IEnumerable<DownloadJob> jobs)
    {
        foreach (var job in jobs)
            Enqueue(job);
    }

    public void PauseJob(Guid jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == jobId);
        if (job != null)
        {
            job.Pause();
            _logger.LogInformation("Paused download: {FileName}", job.FileName);
        }
    }

    public void ResumeJob(Guid jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == jobId);
        if (job != null)
        {
            job.Resume();
            _logger.LogInformation("Resumed download: {FileName}", job.FileName);
        }
    }

    public void CancelJob(Guid jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == jobId);
        if (job != null)
        {
            job.Cancel();
            _logger.LogInformation("Cancelled download: {FileName}", job.FileName);
        }
    }

    public void CancelAll()
    {
        foreach (var job in _jobs.Where(j =>
            j.State is DownloadJobState.Queued or DownloadJobState.Downloading or DownloadJobState.Paused))
        {
            job.Cancel();
        }
        _pending.Clear();
    }

    public void ClearCompleted()
    {
        _jobs.RemoveAll(j =>
            j.State is DownloadJobState.Completed or DownloadJobState.Cancelled or DownloadJobState.Failed);
    }

    private async Task ProcessQueueAsync()
    {
        while (_pending.TryDequeue(out var job))
        {
            await _semaphore.WaitAsync();
            Interlocked.Increment(ref _activeCount);
            JobStarted?.Invoke(job);
            _ = ExecuteJobAsync(job);
        }
    }

    private async Task ExecuteJobAsync(DownloadJob job)
    {
        var progress = new Progress<double>(_ => { });
        await _manager.ExecuteAsync(job, progress);
        _semaphore.Release();
        Interlocked.Decrement(ref _activeCount);
    }

    private void OnJobCompleted(DownloadJob job)
    {
        JobCompleted?.Invoke(job);
    }

    private void OnJobFailed(DownloadJob job)
    {
        JobFailed?.Invoke(job);
    }

    private void OnJobCancelled(DownloadJob job)
    {
        JobCancelled?.Invoke(job);
    }

    private void OnJobProgressChanged(DownloadJob job)
    {
        JobProgressChanged?.Invoke(job);
    }
}
