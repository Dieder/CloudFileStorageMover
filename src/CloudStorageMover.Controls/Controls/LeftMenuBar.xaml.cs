namespace CloudStorageMover.Controls.Controls;

public enum AppMenuItem
{
    Source,
    GooglePhotosPicker,
    Gallery,
    Configuration,
    Transfer,
    Quit
}

public partial class LeftMenuBar : ContentView
{
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(AppMenuItem), typeof(LeftMenuBar),
            AppMenuItem.Source, propertyChanged: OnSelectedItemChanged);

    public event EventHandler<AppMenuItem>? ItemSelected;
    public event EventHandler? QuitRequested;
    public event EventHandler? PhotosPickerRequested;
    public event EventHandler? GalleryRequested;

    public AppMenuItem SelectedItem
    {
        get => (AppMenuItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public LeftMenuBar()
    {
        InitializeComponent();
        UpdateSelectedState(AppMenuItem.Source);
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LeftMenuBar bar && newValue is AppMenuItem item)
            bar.UpdateSelectedState(item);
    }

    private void UpdateSelectedState(AppMenuItem item)
    {
        SourceButton.BackgroundColor = item == AppMenuItem.Source ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
        PhotosPickerButton.BackgroundColor = item == AppMenuItem.GooglePhotosPicker ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
        GalleryButton.BackgroundColor = item == AppMenuItem.Gallery ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
        TransferButton.BackgroundColor = item == AppMenuItem.Transfer ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
        ConfigButton.BackgroundColor = item == AppMenuItem.Configuration ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
    }

    private void OnSourceClicked(object? sender, EventArgs e)
    {
        SelectedItem = AppMenuItem.Source;
        ItemSelected?.Invoke(this, AppMenuItem.Source);
    }

    private void OnPhotosPickerClicked(object? sender, EventArgs e)
    {
        PhotosPickerRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnGalleryClicked(object? sender, EventArgs e)
    {
        GalleryRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnTransferClicked(object? sender, EventArgs e)
    {
        SelectedItem = AppMenuItem.Transfer;
        ItemSelected?.Invoke(this, AppMenuItem.Transfer);
    }

    private void OnConfigClicked(object? sender, EventArgs e)
    {
        SelectedItem = AppMenuItem.Configuration;
        ItemSelected?.Invoke(this, AppMenuItem.Configuration);
    }

    private void OnQuitClicked(object? sender, EventArgs e)
    {
        QuitRequested?.Invoke(this, EventArgs.Empty);
    }
}
