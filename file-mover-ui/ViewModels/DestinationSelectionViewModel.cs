using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FileMover.UI.Models;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class DestinationSelectionViewModel : BaseViewModel
{
    private readonly IFileMoverService fileMoverService;

    [ObservableProperty]
    private StorageProvider? selectedProvider;

    [ObservableProperty]
    private StorageLocation? selectedLocation;

    public ObservableCollection<StorageProvider> StorageProviders { get; } = new();
    public ObservableCollection<StorageLocation> Locations { get; } = new();

    public DestinationSelectionViewModel(IFileMoverService fileMoverService)
    {
        this.fileMoverService = fileMoverService;
        Title = "Select Destination Storage";
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
            foreach (var provider in providers)
                StorageProviders.Add(provider);
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
