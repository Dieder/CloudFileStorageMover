using CommunityToolkit.Mvvm.ComponentModel;

namespace FileMover.UI.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? busyMessage;
}
