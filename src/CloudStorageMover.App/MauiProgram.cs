using System.Text.Json;
using CloudStorageMover.App.Pages;
using CloudStorageMover.App.Services;
using CloudStorageMover.App.ViewModels;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Services;
using CloudStorageMover.Controls.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Load settings
        var settingsService = new AppSettingsService();
        try
        {
            var settingsPath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
            if (File.Exists(settingsPath))
                settingsService.Load(File.ReadAllText(settingsPath));
            else
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                settingsService.Load(reader.ReadToEnd());
            }
        }
        catch
        {
            settingsService.Load("{}");
        }
        builder.Services.AddSingleton(settingsService);

        // Providers (fully qualified to use Core real implementations, not Controls stubs)
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.DesktopStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.GoogleDriveStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.OneDriveStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.MetaStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.PrimeStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.MegaStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.PCloudStorageProvider>();
        builder.Services.AddSingleton<IStorageProviderService, CloudStorageMover.Core.Services.GooglePhotosStorageProvider>();
        builder.Services.AddSingleton<StorageProviderFactory>();

        // Auth services
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.GoogleDriveSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.OneDriveSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.MetaSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.PrimeSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.MegaSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.PCloudSignInService>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.GooglePhotosSignInService>();

        // Download infrastructure
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.DownloadManager>();
        builder.Services.AddSingleton<CloudStorageMover.Core.Services.DownloadQueue>();

        // ViewModels
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<SourceSelectionViewModel>();
        builder.Services.AddTransient<FileSelectionViewModel>();
        builder.Services.AddTransient<TransferViewModel>();
        builder.Services.AddTransient<ConfigurationViewModel>();
        builder.Services.AddTransient<CloudStorageMover.App.ViewModels.PhotoGalleryViewModel>();
        builder.Services.AddTransient<CloudStorageMover.App.ViewModels.CameraImportViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SourceSelectionPage>();
        builder.Services.AddTransient<FileBrowserPage>();
        builder.Services.AddTransient<TransferPage>();
        builder.Services.AddTransient<ConfigurationPage>();
        builder.Services.AddTransient<CloudStorageMover.App.Pages.PhotoGalleryPage>();
        builder.Services.AddTransient<CloudStorageMover.App.Pages.CameraImportPage>();

        // App Services
        var destinationSettings = new DestinationSettingsService(FileSystem.AppDataDirectory);
        destinationSettings.Load();
        builder.Services.AddSingleton(destinationSettings);
        builder.Services.AddSingleton<SelectedFilesService>();
        builder.Services.AddSingleton<CloudStorageMover.App.Services.GooglePhotosPickerService>();
        builder.Services.AddSingleton<IFolderPickerService>(_ =>
        {
#if WINDOWS
            return new CloudStorageMover.App.Platforms.Windows.FolderPickerService();
#else
            return new CloudStorageMover.App.Services.DefaultFolderPickerService();
#endif
        });

        builder.Services.AddSingleton<IOAuthLoopbackService>(_ =>
        {
#if WINDOWS
            return new CloudStorageMover.App.Platforms.Windows.OAuthLoopbackService();
#else
            return new CloudStorageMover.App.Services.DefaultOAuthLoopbackService();
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
