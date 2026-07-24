using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class PhotoGalleryPage : ContentView
{
    private readonly PhotoGalleryViewModel _viewModel;

    public event EventHandler? BackRequested;

    public PhotoGalleryPage(PhotoGalleryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void LoadPhotos(string folderPath)
    {
        _viewModel.LoadFromFolder(folderPath);
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
