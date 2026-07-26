using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class TransferPage : ContentView
{
    private readonly TransferViewModel _viewModel;

    public event EventHandler? BackRequested;

    public TransferPage(TransferViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.BackRequested += OnBackRequested;
    }

    public void Refresh()
    {
        _viewModel.Refresh();
    }

    private void OnBackRequested(object? sender, EventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnDestinationSelected(object? sender, EventArgs e)
    {
        var handler = Application.Current?.Handler;
        if (handler?.MauiContext?.Services == null) return;

        var folderPicker = handler.MauiContext.Services.GetService<CloudStorageMover.Core.Interfaces.IFolderPickerService>();
        if (folderPicker == null) return;

        var result = await folderPicker.PickFolderAsync();
        if (!string.IsNullOrEmpty(result))
        {
            _viewModel.SelectDestinationCommand.Execute(new DestinationItem
            {
                Name = result,
                DisplayLabel = result,
                IsRemovable = false
            });
        }
    }

    private async void OnExecuteTransfer(object? sender, EventArgs e)
    {
        await _viewModel.ExecuteTransferCommand.ExecuteAsync(null);
    }
}
