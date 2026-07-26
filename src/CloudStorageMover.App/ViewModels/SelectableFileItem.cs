using CommunityToolkit.Mvvm.ComponentModel;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.App.ViewModels;

public partial class SelectableFileItem : ObservableObject
{
    public FileItem FileItem { get; }

    [ObservableProperty]
    private bool _isSelected;

    public string Name => FileItem.Name;
    public string FullPath => FileItem.FullPath;
    public long Size => FileItem.Size;
    public bool IsDirectory => FileItem.IsDirectory;
    public string? IconPath => FileItem.IconPath;
    public StorageProviderType ProviderType => FileItem.ProviderType;
    public DateTime ModifiedDate => FileItem.ModifiedDate;

    public string SizeText => Size > 0
        ? Size / 1024.0 / 1024.0 > 1
            ? $"{Size / 1024.0 / 1024.0:F1} MB"
            : $"{Size / 1024.0:F0} KB"
        : "";
    public string Icon => IsSelected ? "\u2611" : "\u2610";

    public SelectableFileItem(FileItem fileItem)
    {
        FileItem = fileItem;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(Icon));
    }
}
