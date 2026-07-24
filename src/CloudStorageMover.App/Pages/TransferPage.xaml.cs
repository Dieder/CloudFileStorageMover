using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class TransferPage : ContentView
{
    public TransferPage(TransferViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
