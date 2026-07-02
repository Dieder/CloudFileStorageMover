using FileMover.UI.Models;

namespace FileMover.UI.Services;

/// <summary>
/// Service interface for file mover operations
/// </summary>
public interface IFileMoverService
{
    /// <summary>
    /// Gets available storage providers
    /// </summary>
    Task<List<StorageProvider>> GetStorageProvidersAsync();
    
    /// <summary>
    /// Gets available locations within a storage provider
    /// </summary>
    Task<List<StorageLocation>> GetLocationsAsync(string providerId);
    
    /// <summary>
    /// Lists files and folders at a specific location
    /// </summary>
    Task<List<FileItem>> ListFilesAsync(string locationId, string path = "");
    
    /// <summary>
    /// Initiates a file transfer
    /// </summary>
    Task<string> InitiateTransferAsync(TransferSession session);
    
    /// <summary>
    /// Gets transfer progress
    /// </summary>
    Task<TransferProgress> GetTransferProgressAsync(string sessionId);
    
    /// <summary>
    /// Cancels a transfer
    /// </summary>
    Task CancelTransferAsync(string sessionId);
    
    /// <summary>
    /// Gets transfer history
    /// </summary>
    Task<List<TransferProgress>> GetTransferHistoryAsync(int limit = 10);
}
