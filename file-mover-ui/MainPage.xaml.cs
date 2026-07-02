using FileMover.UI.Services;

namespace FileMover.UI;

public partial class MainPage : ContentPage
{
	private readonly IFileMoverService fileMoverService;

	public MainPage(IFileMoverService fileMoverService)
	{
		InitializeComponent();
		this.fileMoverService = fileMoverService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadRecentTransfers();
	}

	private async Task LoadRecentTransfers()
	{
		try
		{
			var history = await fileMoverService.GetTransferHistoryAsync(5);
			RecentTransfersCollection.ItemsSource = history;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to load transfer history: {ex.Message}", "OK");
		}
	}

	private async void OnNewTransferClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("source-selection");
	}

	private async void OnViewHistoryClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("transfer-status");
	}
}
