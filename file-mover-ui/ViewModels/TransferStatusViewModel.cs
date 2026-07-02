using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FileMover.UI.Models;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class TransferStatusViewModel : BaseViewModel
{
    private readonly IFileMoverService fileMoverService;
    private CancellationTokenSource? monitorCts;

    [ObservableProperty]
    private TransferProgress? currentTransfer;

    [ObservableProperty]
    private bool isMonitoring;

    public ObservableCollection<TransferProgress> TransferHistory { get; } = new();

    public TransferStatusViewModel(IFileMoverService fileMoverService)
    {
        this.fileMoverService = fileMoverService;
        Title = "Transfer Status";
    }

    public async Task LoadHistoryAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading transfer history...";

            var history = await fileMoverService.GetTransferHistoryAsync();
            TransferHistory.Clear();
            foreach (var transfer in history)
                TransferHistory.Add(transfer);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task MonitorTransferAsync(string sessionId)
    {
        monitorCts = new CancellationTokenSource();
        try
        {
            IsMonitoring = true;
            BusyMessage = "Monitoring transfer...";

            while (!monitorCts.Token.IsCancellationRequested)
            {
                var progress = await fileMoverService.GetTransferProgressAsync(sessionId);
                CurrentTransfer = progress;

                if (progress.Status == TransferStatus.Completed ||
                    progress.Status == TransferStatus.Failed ||
                    progress.Status == TransferStatus.Cancelled)
                {
                    IsMonitoring = false;
                    BusyMessage = progress.Status == TransferStatus.Completed
                        ? "Transfer completed successfully"
                        : $"Transfer {progress.Status.ToString().ToLower()}";
                    break;
                }

                await Task.Delay(1000, monitorCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            BusyMessage = "Monitoring stopped";
        }
        catch (Exception ex)
        {
            BusyMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsMonitoring = false;
        }
    }

    public void StopMonitoring()
    {
        monitorCts?.Cancel();
        IsMonitoring = false;
    }

    public async Task CancelTransferAsync(string sessionId)
    {
        try
        {
            await fileMoverService.CancelTransferAsync(sessionId);
            StopMonitoring();
        }
        catch (Exception ex)
        {
            BusyMessage = $"Error canceling transfer: {ex.Message}";
        }
    }
}
