using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FileMover.UI.Models;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class FileSelectionViewModel : BaseViewModel
{
    private readonly IFileMoverService fileMoverService;
    private readonly Stack<string> pathStack = new();
    private readonly Dictionary<string, List<string>> savedSelectionPaths = new();

    [ObservableProperty]
    private string? currentPath;

    [ObservableProperty]
    private FileItem? selectedFile;

    [ObservableProperty]
    private FileItem? previewFile;

    [ObservableProperty]
    private bool canGoBack;

    public ObservableCollection<FileItem> Files { get; } = new();
    public List<FileItem> AllFiles { get; private set; } = [];
    public ObservableCollection<FileItem> SelectedFiles { get; } = new();

    public FileSelectionViewModel(IFileMoverService fileMoverService)
    {
        this.fileMoverService = fileMoverService;
        Title = "Select Files";
    }

    public async Task LoadFilesAsync(string locationId, string path = "")
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading files...";

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var items = await Task.Run(() => EnumerateFileSystem(path));
                Files.Clear();
                foreach (var item in items)
                    Files.Add(item);
                AllFiles = items;
            }
            else
            {
                var files = await fileMoverService.ListFilesAsync(locationId, path);
                Files.Clear();
                AllFiles = [.. files];
                foreach (var file in AllFiles)
                    Files.Add(file);
            }

            CurrentPath = path;
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task LoadFromFileSystemAsync(string path)
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading...";

            var items = await Task.Run(() => EnumerateFileSystem(path));

            SelectedFiles.Clear();
            if (savedSelectionPaths.TryGetValue(path, out var savedPaths))
            {
                foreach (var item in items.Where(f => savedPaths.Contains(f.Path)))
                {
                    item.IsSelected = true;
                    SelectedFiles.Add(item);
                }
            }

            Files.Clear();
            foreach (var item in items)
                Files.Add(item);
            AllFiles = items;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static List<FileItem> EnumerateFileSystem(string path)
    {
        var items = new List<FileItem>();

        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var dirInfo = new DirectoryInfo(dir);
            items.Add(new FileItem
            {
                Id = dir,
                Name = dirInfo.Name,
                Path = dir,
                IsFolder = true,
                Size = 0,
                Modified = dirInfo.LastWriteTime
            });
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            var fileInfo = new FileInfo(file);
            items.Add(new FileItem
            {
                Id = file,
                Name = fileInfo.Name,
                Path = file,
                IsFolder = false,
                Size = fileInfo.Length,
                Modified = fileInfo.LastWriteTime,
                MimeType = GetMimeType(fileInfo.Extension)
            });
        }

        return items;
    }

    private static string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            _ => "application/octet-stream"
        };
    }

    public async Task NavigateToFolderAsync(FileItem folder)
    {
        if (!folder.IsFolder) return;

        if (CurrentPath != null && SelectedFiles.Count > 0)
            savedSelectionPaths[CurrentPath] = SelectedFiles.Select(f => f.Path).ToList();

        pathStack.Push(CurrentPath ?? "");
        CurrentPath = folder.Path;
        CanGoBack = true;
        await LoadFromFileSystemAsync(folder.Path);
    }

    public async Task NavigateBackAsync()
    {
        if (pathStack.Count == 0) return;

        var parentPath = pathStack.Pop();

        if (CurrentPath != null && SelectedFiles.Count > 0)
            savedSelectionPaths[CurrentPath] = SelectedFiles.Select(f => f.Path).ToList();

        CurrentPath = parentPath;
        CanGoBack = pathStack.Count > 0;

        if (!string.IsNullOrEmpty(parentPath))
            await LoadFromFileSystemAsync(parentPath);
    }

    partial void OnSelectedFileChanged(FileItem? value)
    {
        PreviewFile = value;
    }

    public void ToggleFileSelection(FileItem file)
    {
        file.IsSelected = !file.IsSelected;

        if (file.IsSelected)
        {
            if (!SelectedFiles.Contains(file))
                SelectedFiles.Add(file);
        }
        else
        {
            SelectedFiles.Remove(file);
        }
    }

    public void SelectAll()
    {
        foreach (var file in Files)
        {
            if (!file.IsSelected)
                ToggleFileSelection(file);
        }
    }

    public void DeselectAll()
    {
        foreach (var file in Files.Where(f => f.IsSelected).ToList())
            ToggleFileSelection(file);
    }
}
