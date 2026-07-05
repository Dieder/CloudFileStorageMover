using CloudStorageMover.App.Pages;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Controls.Controls;

namespace CloudStorageMover.App;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly SourceSelectionPage _sourcePage;
    private readonly ConfigurationPage _configPage;

    public MainPage(MainPageViewModel viewModel,
                    SourceSelectionPage sourcePage,
                    ConfigurationPage configPage)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _sourcePage = sourcePage;
        _configPage = configPage;
        BindingContext = viewModel;

        MenuBar.ItemSelected += OnMenuItemSelected;
        MenuBar.QuitRequested += OnQuitRequested;

        ContentArea.Content = _sourcePage;
    }

    private void OnMenuItemSelected(object? sender, AppMenuItem e)
    {
        ContentArea.Content = e switch
        {
            AppMenuItem.Source => _sourcePage,
            AppMenuItem.Configuration => _configPage,
            _ => _sourcePage
        };
    }

    private void OnQuitRequested(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
    }
}
