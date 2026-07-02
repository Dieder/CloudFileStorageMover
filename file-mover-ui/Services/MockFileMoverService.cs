using FileMover.UI.Models;

namespace FileMover.UI.Services;

/// <summary>
/// Mock implementation of IFileMoverService for testing and development
/// </summary>
public class MockFileMoverService : IFileMoverService
{
    public Task<List<StorageProvider>> GetStorageProvidersAsync()
    {
        var providers = new List<StorageProvider>
        {
            new() { Id = "s3", Name = "Amazon S3", Type = "s3", IsConfigured = true },
            new() { Id = "azure", Name = "Azure Blob Storage", Type = "azure", IsConfigured = true },
            new() { Id = "gcs", Name = "Google Cloud Storage", Type = "gcs", IsConfigured = false },
            new() { Id = "local", Name = "Local Device", Type = "local", IsConfigured = true }
        };
        return Task.FromResult(providers);
    }
    
    public Task<List<StorageLocation>> GetLocationsAsync(string providerId)
    {
        if (providerId == "local")
        {
            var locations = new List<StorageLocation>
            {
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
            };
            return Task.FromResult(locations);
        }

        var cloudLocations = new List<StorageLocation>
        {
            new()
            {
                Id = "loc1",
                ProviderId = providerId,
                Path = "/uploads",
                DisplayName = "Uploads",
                LastModified = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "loc2",
                ProviderId = providerId,
                Path = "/documents",
                DisplayName = "Documents",
                LastModified = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "loc3",
                ProviderId = providerId,
                Path = "/photos",
                DisplayName = "Photos",
                LastModified = DateTime.UtcNow
            }
        };
        return Task.FromResult(cloudLocations);
    }
    
    public Task<List<FileItem>> ListFilesAsync(string locationId, string path = "")
    {
        var files = new List<FileItem>
        {
            new()
            {
                Id = "file1",
                Name = "document.pdf",
                Path = "/documents/document.pdf",
                IsFolder = false,
                Size = 1024 * 500,
                Modified = DateTime.UtcNow.AddDays(-1),
                MimeType = "application/pdf"
            },
            new()
            {
                Id = "file2",
                Name = "image.jpg",
                Path = "/documents/image.jpg",
                IsFolder = false,
                Size = 1024 * 1024 * 2,
                Modified = DateTime.UtcNow.AddDays(-2),
                MimeType = "image/jpeg"
            },
            new()
            {
                Id = "folder1",
                Name = "subfolder",
                Path = "/documents/subfolder",
                IsFolder = true,
                Size = 0,
                Modified = DateTime.UtcNow.AddDays(-3)
            }
        };
        return Task.FromResult(files);
    }
    
    public Task<string> InitiateTransferAsync(TransferSession session)
    {
        return Task.FromResult(session.Id);
    }
    
    public Task<TransferProgress> GetTransferProgressAsync(string sessionId)
    {
        var progress = new TransferProgress
        {
            SessionId = sessionId,
            TotalItems = 5,
            CompletedItems = 2,
            TotalBytes = 1024 * 1024 * 100,
            TransferredBytes = 1024 * 1024 * 35,
            ElapsedTime = TimeSpan.FromMinutes(5),
            CurrentFile = "image.jpg",
            Status = TransferStatus.InProgress
        };
        return Task.FromResult(progress);
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
}
