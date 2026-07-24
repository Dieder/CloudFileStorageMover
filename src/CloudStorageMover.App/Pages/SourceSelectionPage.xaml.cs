using System.Collections.ObjectModel;
using CloudStorageMover.Core.Models;
using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class SourceSelectionPage : ContentView
{
    private readonly SourceSelectionViewModel _viewModel;

    public event EventHandler<List<FileItem>>? FoldersLoaded;
    public event EventHandler<StorageProviderType>? ProviderChanging;

    public SourceSelectionPage(SourceSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.FoldersLoaded += OnFoldersLoaded;
        _viewModel.ProviderChanging += OnProviderChanging;
    }

    private void OnFoldersLoaded(object? sender, List<FileItem> folders)
    {
        FoldersLoaded?.Invoke(this, folders);
    }

    private void OnProviderChanging(object? sender, StorageProviderType providerType)
    {
        ProviderChanging?.Invoke(this, providerType);
    }

    private void OnFileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is FileItem item && item.IsDirectory)
        {
            _ = _viewModel.OpenFolderCommand.ExecuteAsync(item);
        }

        if (sender is CollectionView cv)
            cv.SelectedItem = null;
    }
}
