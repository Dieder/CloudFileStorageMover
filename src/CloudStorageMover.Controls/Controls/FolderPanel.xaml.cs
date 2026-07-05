using System.Collections.ObjectModel;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Controls.Controls;

public partial class FolderPanel : ContentView
{
    public static readonly BindableProperty FoldersProperty =
        BindableProperty.Create(nameof(Folders), typeof(ObservableCollection<FileItem>), typeof(FolderPanel),
            new ObservableCollection<FileItem>(), propertyChanged: OnFoldersChanged);

    public event EventHandler<FileItem>? FolderSelected;

    public ObservableCollection<FileItem> Folders
    {
        get => (ObservableCollection<FileItem>)GetValue(FoldersProperty);
        set => SetValue(FoldersProperty, value);
    }

    public FolderPanel()
    {
        InitializeComponent();
        FolderList.ItemsSource = Folders;
    }

    private static void OnFoldersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FolderPanel panel && newValue is ObservableCollection<FileItem> items)
            panel.FolderList.ItemsSource = items;
    }

    private void OnFolderSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is FileItem item)
        {
            FolderSelected?.Invoke(this, item);
        }
    }
}
