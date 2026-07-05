using System.Collections.ObjectModel;
using CloudStorageMover.Core.Models;
using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class SourceSelectionPage : ContentView
{
    private readonly SourceSelectionViewModel _viewModel;

    public event EventHandler<List<FileItem>>? FoldersLoaded;

    public SourceSelectionPage(SourceSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.FoldersLoaded += OnFoldersLoaded;
    }

    private void OnFoldersLoaded(object? sender, List<FileItem> folders)
    {
        FoldersLoaded?.Invoke(this, folders);
    }
}
