using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

[QueryProperty(nameof(SessionId), "sessionId")]
public partial class TransferStatusPage : ContentPage
{
	private readonly TransferStatusViewModel viewModel;
	private string? sessionId;

	public string? SessionId
	{
		get => sessionId;
		set
		{
			sessionId = value;
			if (!string.IsNullOrEmpty(value))
			{
				_ = viewModel.MonitorTransferAsync(value);
			}
		}
	}

	public TransferStatusPage(TransferStatusViewModel viewModel)
	{
		InitializeComponent();
		this.viewModel = viewModel;
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.LoadHistoryAsync();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		viewModel.StopMonitoring();
	}

	private void OnActiveTabClicked(object sender, EventArgs e)
	{
		ActiveTabContent.IsVisible = true;
		HistoryTabContent.IsVisible = false;
		ActiveTabBtn.BackgroundColor = Color.FromArgb("#512BD4");
		ActiveTabBtn.TextColor = Colors.White;
		HistoryTabBtn.BackgroundColor = Colors.Transparent;
		HistoryTabBtn.TextColor = Color.FromArgb("#512BD4");
	}

	private void OnHistoryTabClicked(object sender, EventArgs e)
	{
		ActiveTabContent.IsVisible = false;
		HistoryTabContent.IsVisible = true;
		HistoryTabBtn.BackgroundColor = Color.FromArgb("#512BD4");
		HistoryTabBtn.TextColor = Colors.White;
		ActiveTabBtn.BackgroundColor = Colors.Transparent;
		ActiveTabBtn.TextColor = Color.FromArgb("#512BD4");
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		if (viewModel.CurrentTransfer == null) return;

		bool confirm = await DisplayAlert(
			"Cancel Transfer",
			"Are you sure you want to cancel this transfer?",
			"Yes",
			"No");

		if (confirm)
		{
			await viewModel.CancelTransferAsync(viewModel.CurrentTransfer.SessionId);
		}
	}
}
