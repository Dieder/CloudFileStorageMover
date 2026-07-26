using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.Core.Services;

public class BlobService
{
    private BlobServiceClient? _client;
    private string _connectionString = string.Empty;
    private readonly ILogger<BlobService>? _logger;

    public bool IsConnected => _client != null;
    public string ConnectionString => _connectionString;

    public BlobService(ILogger<BlobService>? logger = null)
    {
        _logger = logger;
    }

    public bool Connect(string connectionString)
    {
        try
        {
            _connectionString = connectionString;
            _client = new BlobServiceClient(connectionString);
            _logger?.LogInformation("Connected to Azure Blob Storage");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to Azure Blob Storage");
            _client = null;
            return false;
        }
    }

    public BlobServiceClient GetClient()
    {
        if (_client == null)
            throw new InvalidOperationException("Not connected. Call Connect() first.");
        return _client;
    }
}
