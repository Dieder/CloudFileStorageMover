using FileMover.UI.Services;
using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

public partial class SourceSelectionPage : ContentPage
{
	private readonly SourceSelectionViewModel viewModel;
	private readonly TransferSessionService sessionService;

	public SourceSelectionPage(SourceSelectionViewModel viewModel, TransferSessionService sessionService)
	{
		InitializeComponent();
		this.viewModel = viewModel;
		this.sessionService = sessionService;
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.LoadProvidersAsync();
	}

	private async void OnNextClicked(object sender, EventArgs e)
	{
		if (viewModel.SelectedLocation == null)
		{
			await DisplayAlert("Selection Required", "Please select a location", "OK");
			return;
		}

		sessionService.SourceLocation = viewModel.SelectedLocation;
		await Shell.Current.GoToAsync("file-selection");
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		sessionService.Reset();
		await Shell.Current.GoToAsync("..");
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("credentials-config");
	}
}
