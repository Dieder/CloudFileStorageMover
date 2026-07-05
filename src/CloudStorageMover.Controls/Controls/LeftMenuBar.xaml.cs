namespace CloudStorageMover.Controls.Controls;

public enum AppMenuItem
{
    Source,
    Configuration,
    Quit
}

public partial class LeftMenuBar : ContentView
{
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(AppMenuItem), typeof(LeftMenuBar),
            AppMenuItem.Source, propertyChanged: OnSelectedItemChanged);

    public event EventHandler<AppMenuItem>? ItemSelected;
    public event EventHandler? QuitRequested;

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
        ConfigButton.BackgroundColor = item == AppMenuItem.Configuration ? Color.FromArgb("#3a3a3a") : Colors.Transparent;
    }

    private void OnSourceClicked(object? sender, EventArgs e)
    {
        SelectedItem = AppMenuItem.Source;
        ItemSelected?.Invoke(this, AppMenuItem.Source);
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
