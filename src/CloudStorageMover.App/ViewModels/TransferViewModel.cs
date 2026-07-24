using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class TransferViewModel : ObservableObject
{
    private readonly DownloadQueue _queue;
    private readonly ILogger<TransferViewModel> _logger;

    public ObservableCollection<DownloadJobItem> ActiveJobs { get; } = [];
    public ObservableCollection<DownloadJobItem> CompletedJobs { get; } = [];

    [ObservableProperty]
    private bool _hasActiveJobs;

    [ObservableProperty]
    private int _activeCount;

    public TransferViewModel(DownloadQueue queue, ILogger<TransferViewModel> logger)
    {
        _queue = queue;
        _logger = logger;

        _queue.JobEnqueued += OnJobChanged;
        _queue.JobProgressChanged += OnJobProgressChanged;
        _queue.JobCompleted += OnJobCompleted;
        _queue.JobFailed += OnJobFailed;
        _queue.JobCancelled += OnJobCancelled;
    }

    private void OnJobChanged(DownloadJob job) => RefreshJobs();
    private void OnJobProgressChanged(DownloadJob job) => RefreshJobs();
    private void OnJobCompleted(DownloadJob job) => RefreshJobs();
    private void OnJobFailed(DownloadJob job) => RefreshJobs();
    private void OnJobCancelled(DownloadJob job) => RefreshJobs();

    private void RefreshJobs()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ActiveJobs.Clear();
            CompletedJobs.Clear();

            foreach (var job in _queue.Jobs)
            {
                var item = new DownloadJobItem(job);
                if (job.State is DownloadJobState.Completed or DownloadJobState.Failed or DownloadJobState.Cancelled)
                    CompletedJobs.Add(item);
                else
                    ActiveJobs.Add(item);
            }

            ActiveCount = _queue.Jobs.Count(j =>
                j.State is DownloadJobState.Queued or DownloadJobState.Downloading or DownloadJobState.Paused);
            HasActiveJobs = ActiveCount > 0;
        });
    }

    [RelayCommand]
    private void PauseJob(Guid jobId) => _queue.PauseJob(jobId);

    [RelayCommand]
    private void ResumeJob(Guid jobId) => _queue.ResumeJob(jobId);

    [RelayCommand]
    private void CancelJob(Guid jobId) => _queue.CancelJob(jobId);

    [RelayCommand]
    private void CancelAll() => _queue.CancelAll();

    [RelayCommand]
    private void ClearCompleted() => _queue.ClearCompleted();
}

public class DownloadJobItem
{
    public Guid JobId { get; }
    public string FileName { get; }
    public string StateText { get; }
    public double Progress { get; }
    public string ProgressText { get; }
    public bool IsPausable { get; }
    public bool IsResumable { get; }
    public bool IsCancellable { get; }

    public DownloadJobItem(DownloadJob job)
    {
        JobId = job.Id;
        FileName = job.FileName;
        Progress = job.Progress;
        ProgressText = $"{job.BytesDownloaded / 1024.0 / 1024.0:F1} MB / {job.TotalBytes / 1024.0 / 1024.0:F1} MB ({(Progress * 100):F0}%)";
        IsPausable = job.State == DownloadJobState.Downloading;
        IsResumable = job.State == DownloadJobState.Paused;
        IsCancellable = job.State is DownloadJobState.Queued or DownloadJobState.Downloading or DownloadJobState.Paused;

        StateText = job.State switch
        {
            DownloadJobState.Queued => "Queued",
            DownloadJobState.Downloading => "Downloading...",
            DownloadJobState.Paused => "Paused",
            DownloadJobState.Completed => "Completed",
            DownloadJobState.Failed => $"Failed: {job.ErrorMessage}",
            DownloadJobState.Cancelled => "Cancelled",
            _ => job.State.ToString()
        };
    }
}
