using FileMover.UI.Models;

namespace FileMover.UI.Services;

public class FileMoverService : IFileMoverService
{
    private readonly IEnumerable<IStorageProviderService> _providers;
    private readonly ProviderConfigLoader _configLoader;
    private readonly TransferSessionService _sessionService;

    public FileMoverService(
        IEnumerable<IStorageProviderService> providers,
        ProviderConfigLoader configLoader,
        TransferSessionService sessionService)
    {
        _providers = providers;
        _configLoader = configLoader;
        _sessionService = sessionService;
    }

    public async Task<List<StorageProvider>> GetStorageProvidersAsync()
    {
        var configs = await _configLoader.LoadAllAsync();
        return configs.Select(c => new StorageProvider
        {
            Id = c.ProviderId,
            Name = c.ProviderName,
            Type = c.ProviderId switch
            {
                "google-drive" => "gcs",
                "windows" => "local",
                "prime" => "prime",
                _ => c.ProviderId
            },
            IsConfigured = true
        }).ToList();
    }

    public async Task<List<StorageLocation>> GetLocationsAsync(string providerId)
    {
        var providerType = providerId switch
        {
            "google-drive" => "gcs",
            "windows" => "local",
            "prime" => "prime",
            _ => providerId
        };
        var provider = GetProvider(providerType);
        if (provider is WindowsStorageProvider)
        {
            return
            [
                new()
                {
                    Id = "local-downloads",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
                    DisplayName = "Downloads",
                    LastModified = DateTime.UtcNow
                },
                new()
                {
                    Id = "local-documents",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    DisplayName = "Documents",
                    LastModified = DateTime.UtcNow
                },
                new()
                {
                    Id = "local-pictures",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    DisplayName = "Pictures",
                    LastModified = DateTime.UtcNow
                },
                new()
                {
                    Id = "local-videos",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    DisplayName = "Videos",
                    LastModified = DateTime.UtcNow
                },
                new()
                {
                    Id = "local-music",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    DisplayName = "Music",
                    LastModified = DateTime.UtcNow
                },
                new()
                {
                    Id = "local-desktop",
                    ProviderId = providerId,
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    DisplayName = "Desktop",
                    LastModified = DateTime.UtcNow
                }
            ];
        }

        var folders = await provider.ListFoldersAsync("root");
        return folders.Select(f => new StorageLocation
        {
            Id = f,
            ProviderId = providerId,
            Path = f,
            DisplayName = f,
            LastModified = DateTime.UtcNow
        }).ToList();
    }

    public async Task<List<FileItem>> ListFilesAsync(string locationId, string path = "")
    {
        var provider = GetProviderForLocation(locationId);
        return await provider.ListFilesAsync(path);
    }

    public Task<string> InitiateTransferAsync(TransferSession session)
    {
        return Task.FromResult(session.Id);
    }

    public Task<TransferProgress> GetTransferProgressAsync(string sessionId)
    {
        return Task.FromResult(new TransferProgress
        {
            SessionId = sessionId,
            TotalItems = 5,
            CompletedItems = 2,
            TotalBytes = 1024 * 1024 * 100,
            TransferredBytes = 1024 * 1024 * 35,
            ElapsedTime = TimeSpan.FromMinutes(5),
            CurrentFile = "image.jpg",
            Status = TransferStatus.InProgress
        });
    }

    public Task CancelTransferAsync(string sessionId)
    {
        return Task.CompletedTask;
    }

    public Task<List<TransferProgress>> GetTransferHistoryAsync(int limit = 10)
    {
        var history = new List<TransferProgress>
        {
            new()
            {
                SessionId = "session1",
                TotalItems = 10,
                CompletedItems = 10,
                TotalBytes = 1024 * 1024 * 250,
                TransferredBytes = 1024 * 1024 * 250,
                ElapsedTime = TimeSpan.FromMinutes(15),
                Status = TransferStatus.Completed
            },
            new()
            {
                SessionId = "session2",
                TotalItems = 5,
                CompletedItems = 3,
                TotalBytes = 1024 * 1024 * 100,
                TransferredBytes = 1024 * 1024 * 60,
                ElapsedTime = TimeSpan.FromMinutes(8),
                Status = TransferStatus.Paused
            }
        };
        return Task.FromResult(history);
    }

    private IStorageProviderService GetProvider(string providerType)
    {
        return _providers.FirstOrDefault(p => p.ProviderType == providerType)
            ?? throw new InvalidOperationException($"No storage provider registered for type: {providerType}");
    }

    private IStorageProviderService GetProviderForLocation(string locationId)
    {
        if (locationId is "local-downloads" or "local-documents" or "local-pictures"
                or "local-videos" or "local-music" or "local-desktop")
            return GetProvider("local");

        var sourceLocation = _sessionService.SourceLocation;
        var destLocation = _sessionService.DestinationLocation;

        var matchedLocation = sourceLocation?.Id == locationId ? sourceLocation
            : destLocation?.Id == locationId ? destLocation
            : null;

        if (matchedLocation is null)
            return GetProvider("local");

        return matchedLocation.ProviderId switch
        {
            "google-drive" => GetProvider("gcs"),
            "prime" => GetProvider("prime"),
            "windows" => GetProvider("local"),
            _ => GetProvider("local")
        };
    }
}
