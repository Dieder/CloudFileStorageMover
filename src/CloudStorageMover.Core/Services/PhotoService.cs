using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CloudStorageMover.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class PhotoService
{
    private readonly BlobService _blobService;
    private readonly ILogger<PhotoService>? _logger;

    public PhotoService(BlobService blobService, ILogger<PhotoService>? logger = null)
    {
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<List<PhotoItem>> ListPhotosAsync(string albumName)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(albumName);
        var photos = new List<PhotoItem>();

        await foreach (var blob in container.GetBlobsAsync())
        {
            if (blob.Name.EndsWith("/")) continue;

            var blobClient = container.GetBlobClient(blob.Name);
            var sasUrl = GenerateSasUrl(blobClient, albumName);

            photos.Add(new PhotoItem
            {
                BlobName = blob.Name,
                AlbumName = albumName,
                ContentType = blob.Properties?.ContentType ?? "unknown",
                Size = blob.Properties?.ContentLength ?? 0,
                LastModified = blob.Properties?.LastModified,
                ThumbnailUrl = sasUrl,
                FullUrl = sasUrl
            });
        }

        return photos.OrderBy(p => p.BlobName).ToList();
    }

    public string GetPhotoUrl(string albumName, string blobName)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(albumName);
        var blobClient = container.GetBlobClient(blobName);
        return GenerateSasUrl(blobClient, albumName);
    }

    private string GenerateSasUrl(BlobClient blobClient, string containerName)
    {
        if (!blobClient.CanGenerateSasUri)
            return blobClient.Uri.ToString();

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task UploadPhotoAsync(string albumName, Stream stream, string fileName, string contentType = "image/jpeg")
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(albumName);
        var blobClient = container.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, overwrite: true);
        _logger?.LogInformation("Uploaded {File} to {Album}", fileName, albumName);
    }

    public async Task<Stream> DownloadPhotoAsync(string albumName, string blobName)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(albumName);
        var blobClient = container.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task DeletePhotoAsync(string albumName, string blobName)
    {
        var client = _blobService.GetClient();
        var container = client.GetBlobContainerClient(albumName);
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
        _logger?.LogInformation("Deleted {Blob} from {Album}", blobName, albumName);
    }

    public async Task DeletePhotosAsync(string albumName, IEnumerable<string> blobNames)
    {
        foreach (var name in blobNames)
        {
            await DeletePhotoAsync(albumName, name);
        }
    }
}
