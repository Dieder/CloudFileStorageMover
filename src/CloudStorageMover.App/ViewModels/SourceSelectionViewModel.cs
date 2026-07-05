using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Controls.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class SourceSelectionViewModel : ObservableObject
{
    private readonly StorageProviderFactory _providerFactory;
    private readonly ILogger<SourceSelectionViewModel> _logger;
    private IStorageProviderService? _selectedProvider;

    [ObservableProperty]
    private bool _isProviderSelected;

    [ObservableProperty]
    private string _selectedProviderName = string.Empty;

    public ObservableCollection<ProviderItem> Providers { get; } = [];

    public IStorageProviderService? SelectedProvider => _selectedProvider;

    public ICommand SelectProviderCommand { get; }

    public event EventHandler<List<FileItem>>? FoldersLoaded;

    public SourceSelectionViewModel(StorageProviderFactory providerFactory,
                                    ILogger<SourceSelectionViewModel> logger)
    {
        _providerFactory = providerFactory;
        _logger = logger;
        SelectProviderCommand = new AsyncRelayCommand<StorageProviderType>(SelectProviderAsync);

        foreach (var p in providerFactory.GetAllProviders())
        {
            Providers.Add(new ProviderItem
            {
                ProviderType = p.ProviderType,
                DisplayName = p.DisplayName
            });
        }
    }

    private async Task SelectProviderAsync(StorageProviderType type)
    {
        _selectedProvider = _providerFactory.GetProvider(type);
        SelectedProviderName = _selectedProvider.DisplayName;
        IsProviderSelected = true;

        try
        {
            var commonDirs = await _selectedProvider.GetCommonDirectoriesAsync();
            var folderItems = commonDirs.Select(d => new FileItem
            {
                Name = Path.GetFileName(d) ?? d,
                FullPath = d,
                IsDirectory = true,
                ProviderType = type
            }).ToList();

            FoldersLoaded?.Invoke(this, folderItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load common directories for {Provider}", type);
        }
    }
}

public class ProviderItem
{
    public StorageProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
