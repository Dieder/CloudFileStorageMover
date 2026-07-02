using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileMover.UI.Models;

namespace FileMover.UI.ViewModels;

public partial class TransferModeViewModel : BaseViewModel
{
    [ObservableProperty]
    private TransferMode selectedMode = TransferMode.CloudToCloud;

    [ObservableProperty]
    private string? serverAddress;

    [ObservableProperty]
    private int serverPort = 5000;

    [ObservableProperty]
    private bool isServer;

    [ObservableProperty]
    private bool isClient;

    [ObservableProperty]
    private string? qrCodeData;

    [ObservableProperty]
    private bool showQrCode;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private ObservableCollection<string> discoveredServers = new();

    public TransferModeViewModel()
    {
        Title = "Transfer Mode";
    }

    [RelayCommand]
    private void StartServer()
    {
        IsServer = true;
        IsClient = false;
        // Generate dummy connection data for QR code display
        var localIp = GetLocalIpAddress();
        QrCodeData = $"FileMover:connect:{localIp}:{ServerPort}:{Guid.NewGuid():N}";
        ShowQrCode = true;
    }

    [RelayCommand]
    private void ConnectToServer()
    {
        IsClient = true;
        IsServer = false;
        ShowQrCode = false;
    }

    [RelayCommand]
    private async Task StartScanning()
    {
        IsScanning = true;
        // In production, this would invoke the ZXing barcode reader
        // to scan a QR code from the server
        await Task.Delay(100);
    }

    [RelayCommand]
    private void StopScanning()
    {
        IsScanning = false;
    }

    [RelayCommand]
    private void UseUsbConnection()
    {
        // Switch to USB pairing mode for Windows desktop
        Title = "USB Pairing";
        ShowQrCode = false;
        IsScanning = false;
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ip?.ToString() ?? "192.168.1.100";
        }
        catch
        {
            return "192.168.1.100";
        }
    }
}
