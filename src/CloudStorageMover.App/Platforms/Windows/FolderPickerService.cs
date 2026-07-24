using CloudStorageMover.Core.Interfaces;

namespace CloudStorageMover.App.Platforms.Windows;

public class FolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select Destination Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return Task.FromResult<string?>(dialog.SelectedPath);
        }

        return Task.FromResult<string?>(null);
    }
}
