using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class ConfigurationViewModel : ObservableObject
{
    private readonly ILogger<ConfigurationViewModel> _logger;

    public ConfigurationViewModel(ILogger<ConfigurationViewModel> logger)
    {
        _logger = logger;
    }
}
