using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CloudStorageMover.Controls.Controls;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private AppMenuItem _selectedMenuItem = AppMenuItem.Source;

    [ObservableProperty]
    private bool _isSourcePageVisible = true;

    [ObservableProperty]
    private bool _isConfigPageVisible;

    [ObservableProperty]
    private bool _isFolderPanelVisible;

    [ObservableProperty]
    private string _currentProviderName = string.Empty;

    public ObservableCollection<FileItem> CurrentFolders { get; } = [];

    public void NavigateTo(AppMenuItem item)
    {
        SelectedMenuItem = item;
        IsSourcePageVisible = item == AppMenuItem.Source;
        IsConfigPageVisible = item == AppMenuItem.Configuration;
    }

    public void SetFolders(List<FileItem> folders, string providerName)
    {
        CurrentFolders.Clear();
        foreach (var folder in folders)
            CurrentFolders.Add(folder);

        CurrentProviderName = providerName;
        IsFolderPanelVisible = true;
        IsSourcePageVisible = true;
    }
}
