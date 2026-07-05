using System.Collections.ObjectModel;
using CloudStorageMover.App.Pages;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Controls.Controls;
using CloudStorageMover.Core.Models;

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
        _sourcePage.FoldersLoaded += OnSourceFoldersLoaded;

        ContentArea.Content = _sourcePage;
    }

    private void OnMenuItemSelected(object? sender, AppMenuItem e)
    {
        _viewModel.NavigateTo(e);

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

    private void OnSourceFoldersLoaded(object? sender, List<FileItem> folders)
    {
        var providerName = _sourcePage.BindingContext is SourceSelectionViewModel svm
            ? svm.SelectedProviderName
            : "Source";

        _viewModel.SetFolders(folders, providerName);
        FolderPanel.Folders = new ObservableCollection<FileItem>(folders);
    }
}
