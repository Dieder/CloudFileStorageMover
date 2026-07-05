using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class ConfigurationPage : ContentView
{
    public ConfigurationPage(ConfigurationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
