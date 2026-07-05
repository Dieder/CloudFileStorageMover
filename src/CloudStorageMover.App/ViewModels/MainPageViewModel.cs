using CommunityToolkit.Mvvm.ComponentModel;
using CloudStorageMover.Controls.Controls;

namespace CloudStorageMover.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private AppMenuItem _selectedMenuItem = AppMenuItem.Source;

    [ObservableProperty]
    private bool _isSourcePageVisible = true;

    [ObservableProperty]
    private bool _isConfigPageVisible;

    public void NavigateTo(AppMenuItem item)
    {
        SelectedMenuItem = item;
        IsSourcePageVisible = item == AppMenuItem.Source;
        IsConfigPageVisible = item == AppMenuItem.Configuration;
    }
}
