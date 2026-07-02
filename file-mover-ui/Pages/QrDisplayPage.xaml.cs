using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

public partial class QrDisplayPage : ContentPage
{
	public QrDisplayPage(TransferModeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	private async void OnDoneClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}
