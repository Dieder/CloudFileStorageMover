using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

public partial class CredentialsConfigPage : ContentPage
{
    public CredentialsConfigPage(CredentialsConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
