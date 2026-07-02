using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using FileMover.UI.Pages;
using FileMover.UI.Services;
using FileMover.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
#if WINDOWS
using Microsoft.UI.Xaml.Controls;
#endif

namespace FileMover.UI;

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

		ScrollViewHandler.Mapper.AppendToMapping("WindowsScrollBar", (h, _) =>
		{
#if WINDOWS
			if (h.PlatformView is ScrollViewer sv)
				sv.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible;
#endif
		});

		// Services
		builder.Services.AddSingleton<IFileMoverService, FileMoverService>();
		builder.Services.AddSingleton<ICredentialsService, CredentialsService>();
		builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
		builder.Services.AddSingleton<TransferSessionService>();
		builder.Services.AddSingleton<ProviderConfigLoader>();
		builder.Services.AddSingleton<IStorageProviderService, WindowsStorageProvider>();
		builder.Services.AddSingleton<IStorageProviderService, GoogleDriveStorageProvider>();
		builder.Services.AddSingleton<IStorageProviderService, PrimeStorageProvider>();

		// ViewModels
		builder.Services.AddTransient<ModeSelectionViewModel>();
		builder.Services.AddTransient<SourceSelectionViewModel>();
		builder.Services.AddTransient<DestinationSelectionViewModel>();
		builder.Services.AddTransient<TransferModeViewModel>();
		builder.Services.AddTransient<FileSelectionViewModel>();
		builder.Services.AddTransient<TransferStatusViewModel>();
		builder.Services.AddTransient<CredentialsConfigViewModel>();

		// Pages
		builder.Services.AddTransient<ModeSelectionPage>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<SourceSelectionPage>();
		builder.Services.AddTransient<DestinationSelectionPage>();
		builder.Services.AddTransient<FileSelectionPage>();
		builder.Services.AddTransient<TransferModePage>();
		builder.Services.AddTransient<ReviewPage>();
		builder.Services.AddTransient<TransferStatusPage>();
		builder.Services.AddTransient<CredentialsConfigPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
