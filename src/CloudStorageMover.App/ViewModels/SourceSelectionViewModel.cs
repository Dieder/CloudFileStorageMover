using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class SourceSelectionViewModel : ObservableObject
{
    private readonly ILogger<SourceSelectionViewModel> _logger;

    public SourceSelectionViewModel(ILogger<SourceSelectionViewModel> logger)
    {
        _logger = logger;
    }
}
