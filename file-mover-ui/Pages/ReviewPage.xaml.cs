using FileMover.UI.Services;

namespace FileMover.UI.Pages;

public partial class ReviewPage : ContentPage
{
	private readonly TransferSessionService sessionService;
	private readonly IFileMoverService fileMoverService;

	public ReviewPage(TransferSessionService sessionService, IFileMoverService fileMoverService)
	{
		InitializeComponent();
		this.sessionService = sessionService;
		this.fileMoverService = fileMoverService;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		LoadReviewData();
	}

	private void LoadReviewData()
	{
		SourceLabel.Text = sessionService.SourceLocation?.DisplayName ?? "Not selected";
		SourcePathLabel.Text = sessionService.SourceLocation?.Path ?? "";

		DestinationLabel.Text = sessionService.DestinationLocation?.DisplayName ?? "Not selected";
		DestinationPathLabel.Text = sessionService.DestinationLocation?.Path ?? "";

		var fileCount = sessionService.SelectedFiles.Count;
		FileCountLabel.Text = $"{fileCount} file(s) selected";

		var totalSize = sessionService.SelectedFiles.Sum(f => f.Size);
		TotalSizeLabel.Text = $"Total size: {FormatSize(totalSize)}";

		ModeLabel.Text = sessionService.SelectedTransferMode switch
		{
			Models.TransferMode.CloudToCloud => "Cloud-to-Cloud Transfer",
			Models.TransferMode.WiFiDirect => "WiFi Direct Transfer",
			Models.TransferMode.CloudToLocal => "Cloud to Local",
			Models.TransferMode.LocalToCloud => "Local to Cloud",
			_ => "Unknown"
		};

		if (sessionService.SelectedTransferMode == Models.TransferMode.WiFiDirect)
		{
			ConnectionLabel.IsVisible = true;
			if (sessionService.IsServerMode)
			{
				ConnectionLabel.Text = $"Mode: Server (listening on port {sessionService.ServerPort})";
			}
			else
			{
				ConnectionLabel.Text = $"Mode: Client (connecting to {sessionService.ServerAddress}:{sessionService.ServerPort})";
			}
		}
	}

	private async void OnStartTransferClicked(object sender, EventArgs e)
	{
		try
		{
			StartTransferBtn.IsEnabled = false;
			StartTransferBtn.Text = "Starting...";

			var session = sessionService.CreateSession();
			var sessionId = await fileMoverService.InitiateTransferAsync(session);

			await Shell.Current.GoToAsync($"transfer-status?sessionId={sessionId}");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to start transfer: {ex.Message}", "OK");
			StartTransferBtn.IsEnabled = true;
			StartTransferBtn.Text = "Start Transfer";
		}
	}

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}

	private static string FormatSize(long bytes)
	{
		string[] sizes = { "B", "KB", "MB", "GB" };
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < sizes.Length - 1)
		{
			order++;
			len /= 1024;
		}
		return $"{len:0.##} {sizes[order]}";
	}
}
