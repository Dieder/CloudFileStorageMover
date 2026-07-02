using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FileMover.UI.Models;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class FileSelectionViewModel : BaseViewModel
{
    private readonly IFileMoverService fileMoverService;
    private readonly TransferSessionService sessionService;
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

    public bool IsCloudProvider { get; private set; }
    public string ActiveLocationId { get; private set; } = string.Empty;

    public ObservableCollection<FileItem> Files { get; } = new();
    public List<FileItem> AllFiles { get; private set; } = [];
    public ObservableCollection<FileItem> SelectedFiles { get; } = new();

    public FileSelectionViewModel(IFileMoverService fileMoverService, TransferSessionService sessionService)
    {
        this.fileMoverService = fileMoverService;
        this.sessionService = sessionService;
        Title = "Select Files";
    }

    public async Task LoadFilesAsync(string locationId, string path = "")
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading files...";

            var source = sessionService.SourceLocation;
            var dest = sessionService.DestinationLocation;
            var matched = source?.Id == locationId ? source : dest?.Id == locationId ? dest : null;
            var isLocal = matched?.ProviderId == "windows";

            if (isLocal && !string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var items = await Task.Run(() => EnumerateFileSystem(path));
                items = InjectSystemFolderEntries(items, path);
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

            IsCloudProvider = !isLocal;
            ActiveLocationId = locationId;
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
            items = InjectSystemFolderEntries(items, path);

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

    private static bool IsDesktopPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return string.Equals(
            Path.GetFullPath(path),
            Path.GetFullPath(desktopPath),
            StringComparison.OrdinalIgnoreCase);
    }

    private static readonly (string Name, Func<string> GetPath)[] SystemFolderEntries =
    [
        ("Downloads", () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")),
        ("Documents", () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
        ("Pictures", () => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
        ("Videos", () => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
        ("Music", () => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
    ];

    private static List<FileItem> InjectSystemFolderEntries(List<FileItem> rawItems, string path)
    {
        if (!IsDesktopPath(path))
            return rawItems;

        var result = new List<FileItem>(rawItems.Count + SystemFolderEntries.Length);

        foreach (var entry in SystemFolderEntries)
        {
            var folderPath = entry.GetPath();
            if (Directory.Exists(folderPath))
            {
                result.Add(new FileItem
                {
                    Id = folderPath,
                    Name = entry.Name,
                    Path = folderPath,
                    IsFolder = true,
                    Size = 0,
                    Modified = Directory.GetLastWriteTime(folderPath)
                });
            }
        }

        result.AddRange(rawItems);
        return result;
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

        if (IsCloudProvider)
        {
            await LoadFilesAsync(ActiveLocationId, folder.Path);
        }
        else
        {
            await LoadFromFileSystemAsync(folder.Path);
        }
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
        {
            if (IsCloudProvider)
            {
                await LoadFilesAsync(ActiveLocationId, parentPath);
            }
            else
            {
                await LoadFromFileSystemAsync(parentPath);
            }
        }
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
