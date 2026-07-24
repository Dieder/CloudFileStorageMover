using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace CloudStorageMover.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<App>>();
        
        logger.LogInformation("=== APPLICATION WINDOW CREATION STARTED ===");
        
        try
        {
            logger.LogInformation("Requesting storage permissions");
            RequestStoragePermissions();
            logger.LogInformation("Storage permissions granted");
            
            logger.LogInformation("Creating AppShell window");
            var window = new Window(new AppShell());
            
            logger.LogInformation("=== APPLICATION WINDOW CREATED SUCCESSFULLY ===");
            return window;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "=== APPLICATION WINDOW CREATION FAILED - FALLING BACK ===");
            
            var fallback = new Window(new AppShell());
            
            logger.LogCritical("=== FALLBACK WINDOW CREATED, APP MAY HAVE ISSUES ===");
            return fallback;
        }
    }

    private static async void RequestStoragePermissions()
    {
        #if ANDROID
        try
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("Requesting Android storage read permissions");
            await Permissions.RequestAsync<Permissions.StorageRead>();
            logger.LogInformation("Android storage read permissions granted");
            
            logger.LogInformation("Requesting Android storage write permissions");
            await Permissions.RequestAsync<Permissions.StorageWrite>();
            logger.LogInformation("Android storage write permissions granted");
        }
        catch (Exception ex)
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<App>>();
            logger.LogError(ex, "Failed to request Android storage permissions during app creation");
            throw;
        }
        #endif
    }
}