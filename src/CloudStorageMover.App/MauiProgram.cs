using CloudStorageMover.App.Pages;
using CloudStorageMover.App.ViewModels;
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

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<SourceSelectionViewModel>();
        builder.Services.AddTransient<ConfigurationViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SourceSelectionPage>();
        builder.Services.AddTransient<ConfigurationPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
