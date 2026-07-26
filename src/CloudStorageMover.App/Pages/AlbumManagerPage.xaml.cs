using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class AlbumManagerPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public AlbumManagerPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
