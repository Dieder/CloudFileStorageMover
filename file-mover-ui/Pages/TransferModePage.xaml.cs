using FileMover.UI.Models;
using FileMover.UI.Services;
using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

	[QueryProperty(nameof(Mode), "mode")]
public partial class TransferModePage : ContentPage
{
	private readonly TransferModeViewModel viewModel;
	private readonly TransferSessionService sessionService;

	public TransferModePage(TransferModeViewModel viewModel, TransferSessionService sessionService)
	{
		InitializeComponent();
		this.viewModel = viewModel;
		this.sessionService = sessionService;
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (!string.IsNullOrEmpty(Mode))
		{
			if (Mode.Equals("client", StringComparison.OrdinalIgnoreCase))
			{
				WiFiDirectRadio.IsChecked = true;
				ClientRadio.IsChecked = true;
			}
			Mode = null;
		}
	}

	public string? Mode { get; set; }

	private void OnWiFiModeChanged()
	{
		bool isWiFi = WiFiDirectRadio.IsChecked;
		ShowQrBtn.IsVisible = isWiFi && ServerRadio.IsChecked;
		ScanQrBtn.IsVisible = isWiFi && ClientRadio.IsChecked;
		UsbPairingBtn.IsVisible = isWiFi;
	}

	private void OnServerRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
			OnWiFiModeChanged();
	}

	private void OnClientRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
			OnWiFiModeChanged();
	}

	private async void OnShowQrClicked(object sender, EventArgs e)
	{
		var localIp = GetLocalIpAddress();
		viewModel.QrCodeData = $"FileMover:connect:{localIp}:{sessionService.ServerPort}:{Guid.NewGuid():N}";
		await Shell.Current.GoToAsync("qr-display");
	}

	private async void OnScanQrClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("qr-scanner");
	}

	private async void OnUsbPairingClicked(object sender, EventArgs e)
	{
		await DisplayAlert("USB Pairing (Windows)",
			"1. Connect both devices via USB cable.\n" +
			"2. On the server device, enable 'USB tethering' in network settings.\n" +
			"3. The client should detect the server automatically.\n" +
			"4. Confirm the pairing on both devices.\n\n" +
			"Note: Both devices must have USB debugging enabled.",
			"OK");
	}

	private async void OnContinueClicked(object sender, EventArgs e)
	{
		if (CloudToCloudRadio.IsChecked)
		{
			sessionService.SelectedTransferMode = TransferMode.CloudToCloud;
			await Shell.Current.GoToAsync("review");
		}
		else if (WiFiDirectRadio.IsChecked)
		{
			if (!ServerRadio.IsChecked && !ClientRadio.IsChecked)
			{
				await DisplayAlert("Selection Required", "Please select Server or Client mode", "OK");
				return;
			}

			sessionService.SelectedTransferMode = TransferMode.WiFiDirect;
			sessionService.IsServerMode = ServerRadio.IsChecked;

			if (ClientRadio.IsChecked)
			{
				if (string.IsNullOrWhiteSpace(ServerAddressEntry.Text))
				{
					await DisplayAlert("Address Required", "Please enter server address", "OK");
					return;
				}
				sessionService.ServerAddress = ServerAddressEntry.Text.Trim();
			}

			if (int.TryParse(ServerPortEntry.Text, out int port))
			{
				sessionService.ServerPort = port;
			}

			await Shell.Current.GoToAsync("review");
		}
		else
		{
			await DisplayAlert("Selection Required", "Please select a transfer mode", "OK");
		}
	}

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
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
