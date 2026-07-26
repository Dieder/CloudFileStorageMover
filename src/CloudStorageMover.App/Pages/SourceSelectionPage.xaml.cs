using System.Collections.ObjectModel;
using CloudStorageMover.Core.Models;
using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class SourceSelectionPage : ContentView
{
    private readonly SourceSelectionViewModel _viewModel;

    public event EventHandler<List<FileItem>>? FoldersLoaded;
    public event EventHandler<StorageProviderType>? ProviderChanging;
    public event EventHandler? TransferRequested;

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

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not SelectableFileItem item)
            return;

        if (_viewModel.IsMultiSelectMode)
        {
            _viewModel.ToggleItemSelectionCommand.Execute(item);
            return;
        }

        if (item.IsDirectory)
        {
            _ = _viewModel.OpenFolderCommand.ExecuteAsync(item.FileItem);
        }
    }

    private void OnTransferClicked(object? sender, EventArgs e)
    {
        TransferRequested?.Invoke(this, EventArgs.Empty);
    }
}
