using Azure;
using Azure.Storage.Blobs;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class AlbumService
{
    private readonly BlobService _blobService;
    private readonly ILogger<AlbumService>? _logger;

    public AlbumService(BlobService blobService, ILogger<AlbumService>? logger = null)
    {
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<List<AlbumModel>> ListAlbumsAsync()
    {
        var client = _blobService.GetClient();
        var albums = new List<AlbumModel>();

        await foreach (var container in client.GetBlobContainersAsync())
        {
            var blobCount = await GetBlobCountAsync(container.Name);
            albums.Add(new AlbumModel
            {
                Name = container.Name,
                Uri = client.GetBlobContainerClient(container.Name).Uri.ToString(),
                PhotoCount = blobCount,
                LastModified = container.Properties.LastModified
            });
        }

        return albums.OrderBy(a => a.Name).ToList();
    }

    public async Task<AlbumModel> CreateAlbumAsync(string name)
    {
        var client = _blobService.GetClient();
        var sanitizedName = SanitizeName(name);
        var container = await client.CreateBlobContainerAsync(sanitizedName);

        _logger?.LogInformation("Created album: {Name}", sanitizedName);

        return new AlbumModel
        {
            Name = sanitizedName,
            Uri = container.Value.Uri.ToString(),
            PhotoCount = 0,
            LastModified = DateTimeOffset.UtcNow
        };
    }

    public async Task DeleteAlbumAsync(string name)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(name);
        await container.DeleteIfExistsAsync();
        _logger?.LogInformation("Deleted album: {Name}", name);
    }

    public async Task RenameAlbumAsync(string oldName, string newName)
    {
        var client = _blobService.GetClient();
        var sanitizedNewName = SanitizeName(newName);
        var oldContainer = client.GetBlobContainerClient(oldName);
        var newContainer = await client.CreateBlobContainerAsync(sanitizedNewName);

        await foreach (var blob in oldContainer.GetBlobsAsync())
        {
            var sourceBlob = oldContainer.GetBlobClient(blob.Name);
            var destBlob = newContainer.Value.GetBlobClient(blob.Name);
            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
        }

        // Wait for copy to complete then delete old
        await Task.Delay(1000);
        await oldContainer.DeleteIfExistsAsync();

        _logger?.LogInformation("Renamed album: {Old} → {New}", oldName, sanitizedNewName);
    }

    public async Task<int> GetBlobCountAsync(string containerName)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(containerName);
        var count = 0;
        await foreach (var _ in container.GetBlobsAsync())
            count++;
        return count;
    }

    private static string SanitizeName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray()).ToLowerInvariant();
    }
}
