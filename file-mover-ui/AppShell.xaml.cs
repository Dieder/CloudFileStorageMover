using FileMover.UI.Pages;

namespace FileMover.UI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("mode-selection", typeof(ModeSelectionPage));
		Routing.RegisterRoute("home", typeof(HomePage));
		Routing.RegisterRoute("source-selection", typeof(SourceSelectionPage));
		Routing.RegisterRoute("destination-selection", typeof(DestinationSelectionPage));
		Routing.RegisterRoute("file-selection", typeof(FileSelectionPage));
		Routing.RegisterRoute("transfer-mode", typeof(TransferModePage));
		Routing.RegisterRoute("review", typeof(ReviewPage));
		Routing.RegisterRoute("transfer-status", typeof(TransferStatusPage));
		Routing.RegisterRoute("qr-display", typeof(QrDisplayPage));
		Routing.RegisterRoute("qr-scanner", typeof(QrScannerPage));
		Routing.RegisterRoute("image-editor", typeof(ImageEditorPage));
		Routing.RegisterRoute("video-player", typeof(VideoPlayerPage));
		Routing.RegisterRoute("credentials-config", typeof(CredentialsConfigPage));
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("credentials-config");
	}
}
