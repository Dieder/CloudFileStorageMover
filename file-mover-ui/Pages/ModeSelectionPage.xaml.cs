namespace FileMover.UI.Pages;

public partial class ModeSelectionPage : ContentPage
{
    public ModeSelectionPage()
    {
        InitializeComponent();
    }

    private async void OnServerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("source-selection");
    }

    private async void OnClientClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("transfer-mode?mode=client");
    }
}
