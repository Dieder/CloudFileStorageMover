using CloudStorageMover.Core.Interfaces;

namespace CloudStorageMover.App.Services;

public class DefaultFolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select any file in the destination folder"
            });

            if (result != null)
            {
                return Path.GetDirectoryName(result.FullPath);
            }
        }
        catch
        {
        }

        return null;
    }
}
