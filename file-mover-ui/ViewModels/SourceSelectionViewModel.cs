using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FileMover.UI.Models;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class SourceSelectionViewModel : BaseViewModel
{
    private readonly IFileMoverService fileMoverService;

    private readonly ICredentialsService credentialsService;

    [ObservableProperty]
    private StorageProvider? selectedProvider;

    [ObservableProperty]
    private StorageLocation? selectedLocation;

    public ObservableCollection<StorageProvider> StorageProviders { get; } = new();
    public ObservableCollection<StorageLocation> Locations { get; } = new();

    public ICommand ConfigureProviderCommand { get; }

    public SourceSelectionViewModel(IFileMoverService fileMoverService, ICredentialsService credentialsService)
    {
        this.fileMoverService = fileMoverService;
        this.credentialsService = credentialsService;
        Title = "Select Source Storage";
        ConfigureProviderCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("credentials-config");
        });
    }

    partial void OnSelectedProviderChanged(StorageProvider? value)
    {
        if (value != null)
        {
            _ = LoadLocationsAsync(value.Id);
        }
    }

    public async Task LoadProvidersAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading storage providers...";

            var providers = await fileMoverService.GetStorageProvidersAsync();
            StorageProviders.Clear();

            var providerIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["gcs"] = "google-drive"
            };

            foreach (var provider in providers)
            {
                if (provider.Type == "local")
                {
                    provider.IsConfigured = true;
                }
                else if (providerIdMap.TryGetValue(provider.Type, out var credId))
                {
                    provider.IsConfigured = await credentialsService.IsConfiguredAsync(credId);
                }
                StorageProviders.Add(provider);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadLocationsAsync(string providerId)
    {
        try
        {
            var provider = StorageProviders.FirstOrDefault(p => p.Id == providerId);
            IsBusy = true;
            BusyMessage = $"Loading locations for {provider?.Name ?? providerId}...";

            var locations = await fileMoverService.GetLocationsAsync(providerId);
            Locations.Clear();
            foreach (var location in locations)
                Locations.Add(location);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
