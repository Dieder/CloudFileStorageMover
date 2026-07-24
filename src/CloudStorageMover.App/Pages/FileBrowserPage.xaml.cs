using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.App.Pages;

    public partial class FileBrowserPage : ContentView
{
    private readonly FileSelectionViewModel _viewModel;

    public event Action? NavigateToTransferRequested;
    public event EventHandler<FileItem>? FolderSelected;

    public FileBrowserPage(FileSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.NavigateToTransferRequested += () => NavigateToTransferRequested?.Invoke();
    }

    public async Task LoadFolderAsync(IStorageProviderService provider, FileItem folder)
    {
        await _viewModel.LoadFolderAsync(provider, folder);
    }

    public void SetFolder(FileItem folder)
    {
        FolderSelected?.Invoke(this, folder);
    }
}
