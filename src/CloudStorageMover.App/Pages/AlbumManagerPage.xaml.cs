using CloudStorageMover.App.ViewModels;

namespace CloudStorageMover.App.Pages;

public partial class AlbumManagerPage : ContentView
{
    private readonly MainViewModel _viewModel;

    public AlbumManagerPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void OnDrop(object? sender, DropEventArgs e)
    {
        try
        {
            var filePaths = await ExtractFilePathsAsync(e);
            if (filePaths.Count > 0)
            {
                await _viewModel.UploadDroppedFilesCommand.ExecuteAsync(filePaths);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Drop handling error: {ex.Message}");
        }
    }

    private static async Task<List<string>> ExtractFilePathsAsync(DropEventArgs e)
    {
        var filePaths = new List<string>();

#if WINDOWS
        if (e.PlatformArgs?.DragEventArgs?.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems) == true)
        {
            var items = await e.PlatformArgs.DragEventArgs.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is Windows.Storage.StorageFile file)
                    filePaths.Add(file.Path);
                else if (item is Windows.Storage.StorageFolder folder)
                {
                    try
                    {
                        var files = await folder.GetFilesAsync();
                        foreach (var f in files)
                            filePaths.Add(f.Path);
                    }
                    catch { }
                }
            }
        }
#elif IOS || MACCATALYST
        var session = e.PlatformArgs?.DropSession;
        if (session != null)
        {
            foreach (var item in session.Items)
            {
                var result = await LoadItemAsync(item.ItemProvider, item.ItemProvider.RegisteredTypeIdentifiers.ToList());
                if (result?.FileUrl?.Path is not null)
                    filePaths.Add(result.FileUrl.Path);
            }
        }
#endif

        if (filePaths.Count == 0)
        {
            var text = await e.Data.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(text) && File.Exists(text.Trim()))
            {
                filePaths.Add(text.Trim());
            }
        }

        return filePaths;
    }

#if IOS || MACCATALYST
    private static async Task<Foundation.NSUrl?> LoadItemAsync(Foundation.NSItemProvider itemProvider, List<string> typeIdentifiers)
    {
        if (typeIdentifiers is null || typeIdentifiers.Count == 0)
            return null;

        var typeIdent = typeIdentifiers.First();

        if (itemProvider.HasItemConformingTo(typeIdent))
            return await itemProvider.LoadInPlaceFileRepresentationAsync(typeIdent);

        typeIdentifiers.Remove(typeIdent);
        return await LoadItemAsync(itemProvider, typeIdentifiers);
    }
#endif
}
