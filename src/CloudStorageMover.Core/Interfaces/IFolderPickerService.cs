namespace CloudStorageMover.Core.Interfaces;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}
