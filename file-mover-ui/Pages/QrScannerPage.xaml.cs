using FileMover.UI.Services;
using ZXing.Net.Maui;

namespace FileMover.UI.Pages;

public partial class QrScannerPage : ContentPage
{
	private readonly TransferSessionService sessionService;
	private bool isProcessing;

	public QrScannerPage(TransferSessionService sessionService)
	{
		InitializeComponent();
		this.sessionService = sessionService;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		isProcessing = false;
		BarcodeReader.IsDetecting = true;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		BarcodeReader.IsDetecting = false;
	}

	private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
	{
		if (isProcessing) return;

		var result = e.Results?.FirstOrDefault();
		if (result?.Value == null) return;

		isProcessing = true;
		BarcodeReader.IsDetecting = false;

		await MainThread.InvokeOnMainThreadAsync(async () =>
		{
			try
			{
				var data = result.Value;
				StatusLabel.Text = "Connecting...";

				// Expected format: "FileMover:connect:{ip}:{port}:{token}"
				if (data.StartsWith("FileMover:connect:"))
				{
					var parts = data.Split(':');
					if (parts.Length >= 4)
					{
						sessionService.ServerAddress = parts[2];
						if (int.TryParse(parts[3], out int port))
						{
							sessionService.ServerPort = port;
						}

						await DisplayAlert("Connected",
							$"Connected to server at {sessionService.ServerAddress}:{sessionService.ServerPort}",
							"OK");

						await Shell.Current.GoToAsync("..");
						return;
					}
				}

				await DisplayAlert("Invalid Code", "The scanned QR code is not a valid FileMover connection code", "OK");
			}
			finally
			{
				isProcessing = false;
			}
		});
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}
