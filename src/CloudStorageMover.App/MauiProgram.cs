using CloudStorageMover.App.Pages;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Controls.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Providers
        builder.Services.AddSingleton<DesktopStorageProvider>();
        builder.Services.AddSingleton<GoogleDriveStorageProvider>();
        builder.Services.AddSingleton<OneDriveStorageProvider>();
        builder.Services.AddSingleton<MetaStorageProvider>();
        builder.Services.AddSingleton<PrimeStorageProvider>();
        builder.Services.AddSingleton<StorageProviderFactory>();

        // ViewModels
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<SourceSelectionViewModel>();
        builder.Services.AddTransient<ConfigurationViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SourceSelectionPage>();
        builder.Services.AddTransient<ConfigurationPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
