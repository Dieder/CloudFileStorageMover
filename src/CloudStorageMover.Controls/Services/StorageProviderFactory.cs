using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.Controls.Services;

public class StorageProviderFactory
{
    private readonly IEnumerable<IStorageProviderService> _providers;

    public StorageProviderFactory(IEnumerable<IStorageProviderService> providers)
    {
        _providers = providers;
    }

    public IStorageProviderService GetProvider(StorageProviderType type)
    {
        return _providers.FirstOrDefault(p => p.ProviderType == type)
            ?? throw new InvalidOperationException($"No provider registered for {type}");
    }

    public List<IStorageProviderService> GetAllProviders()
    {
        return _providers.ToList();
    }
}
