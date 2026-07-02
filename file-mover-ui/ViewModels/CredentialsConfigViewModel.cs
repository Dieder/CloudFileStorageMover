using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileMover.UI.Services;

namespace FileMover.UI.ViewModels;

public partial class CredentialsConfigViewModel : BaseViewModel
{
    private readonly ICredentialsService credentialsService;

    [ObservableProperty]
    private ConfigurableProvider? selectedProvider;

    [ObservableProperty]
    private bool isProviderSelected;

    [ObservableProperty]
    private bool isConfigured;

    [ObservableProperty]
    private string field1Label = "";

    [ObservableProperty]
    private string field1Value = "";

    [ObservableProperty]
    private string field2Label = "";

    [ObservableProperty]
    private string field2Value = "";

    public ObservableCollection<ConfigurableProvider> ConfigurableProviders { get; } = new();

    public CredentialsConfigViewModel(ICredentialsService credentialsService)
    {
        this.credentialsService = credentialsService;
        Title = "Credentials Configuration";
        LoadProviders();
    }

    private void LoadProviders()
    {
        ConfigurableProviders.Add(new ConfigurableProvider
        {
            Id = "google-drive",
            Name = "Google Drive",
            Field1Label = "Client ID",
            Field2Label = "Client Secret"
        });
        ConfigurableProviders.Add(new ConfigurableProvider
        {
            Id = "prime",
            Name = "Amazon Prime",
            Field1Label = "API Key",
            Field2Label = "API Secret"
        });
    }

    partial void OnSelectedProviderChanged(ConfigurableProvider? value)
    {
        if (value == null)
        {
            IsProviderSelected = false;
            return;
        }

        IsProviderSelected = true;
        Field1Label = value.Field1Label;
        Field2Label = value.Field2Label;
        _ = LoadExistingCredentialsAsync(value.Id);
    }

    private async Task LoadExistingCredentialsAsync(string providerId)
    {
        var creds = await credentialsService.GetCredentialsAsync(providerId);
        if (creds != null)
        {
            Field1Value = creds.TryGetValue("field1", out var f1) ? f1 : "";
            Field2Value = creds.TryGetValue("field2", out var f2) ? f2 : "";
            IsConfigured = true;
        }
        else
        {
            Field1Value = "";
            Field2Value = "";
            IsConfigured = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProvider == null) return;

        var credentials = new Dictionary<string, string>
        {
            ["field1"] = Field1Value ?? "",
            ["field2"] = Field2Value ?? ""
        };

        await credentialsService.SaveCredentialsAsync(SelectedProvider.Id, credentials);
        IsConfigured = true;
        SelectedProvider.IsConfigured = true;

        await Shell.Current.DisplayAlert("Saved", $"Credentials saved for {SelectedProvider.Name}", "OK");
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        if (SelectedProvider == null) return;

        await credentialsService.ClearCredentialsAsync(SelectedProvider.Id);
        Field1Value = "";
        Field2Value = "";
        IsConfigured = false;
        SelectedProvider.IsConfigured = false;

        await Shell.Current.DisplayAlert("Cleared", $"Credentials cleared for {SelectedProvider.Name}", "OK");
    }
}

public class ConfigurableProvider
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Field1Label { get; set; } = "";
    public string Field2Label { get; set; } = "";
    public bool IsConfigured { get; set; }
}
