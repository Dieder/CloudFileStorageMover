using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class SourceSelectionPage : ContentView
{
    public SourceSelectionPage(SourceSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
