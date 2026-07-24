using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CG.Web.MegaApiClient;

namespace CloudStorageMover.Core.Services;

public class MegaStorageProvider : IStorageProviderService
{
    private readonly MegaSignInService _authService;

    public StorageProviderType ProviderType => StorageProviderType.Mega;
    public string DisplayName => "MEGA";

    public MegaStorageProvider(MegaSignInService authService)
    {
        _authService = authService;
    }

    public async Task<List<FileItem>> ListFoldersAsync(string path, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to list folders.");

        var client = _authService.GetClient();
        var parent = await ResolveNodeAsync(client, path);
        if (parent == null)
            return new List<FileItem>();

        var childNodes = (await client.GetNodesAsync(parent))
            .Where(n => n.Type == NodeType.Directory);

        return childNodes.Select(n => new FileItem
        {
            Name = n.Name,
            FullPath = n.Id,
            IsDirectory = true,
            ProviderType = ProviderType
        }).ToList();
    }

    public async Task<List<FileItem>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to list files.");

        var client = _authService.GetClient();
        var parent = await ResolveNodeAsync(client, path);
        if (parent == null)
            return new List<FileItem>();

        var childNodes = (await client.GetNodesAsync(parent))
            .Where(n => n.Type == NodeType.File);

        return childNodes.Select(n => new FileItem
        {
            Name = n.Name,
            FullPath = n.Id,
            Size = n.Size,
            ModifiedDate = n.ModificationDate ?? DateTime.MinValue,
            IsDirectory = false,
            ProviderType = ProviderType
        }).ToList();
    }

    public async Task<List<string>> GetCommonDirectoriesAsync(CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            return new List<string>();

        var client = _authService.GetClient();
        var allNodes = await client.GetNodesAsync();
        var root = allNodes.SingleOrDefault(n => n.Type == NodeType.Root);
        if (root != null)
            return new List<string> { $"{root.Id}|MEGA Cloud Drive" };

        return new List<string> { "/|MEGA Cloud Drive" };
    }

    public async Task<Stream> OpenReadStreamAsync(string remotePath, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to open a file stream.");

        var client = _authService.GetClient();
        var allNodes = await client.GetNodesAsync();
        var node = FindNodeByPath(allNodes, remotePath);
        if (node == null || node.Type != NodeType.File)
            throw new InvalidOperationException("File not found.");

        return await client.DownloadAsync(node);
    }

    public async Task UploadAsync(string localPath, string remotePath, Stream content, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to upload files.");

        var client = _authService.GetClient();
        var remoteDir = Path.GetDirectoryName(remotePath)?.Replace('\\', '/') ?? "";
        var fileName = Path.GetFileName(remotePath);

        INode parent;
        if (string.IsNullOrEmpty(remoteDir))
        {
            var allNodes = await client.GetNodesAsync();
            parent = allNodes.Single(n => n.Type == NodeType.Root);
        }
        else
        {
            parent = await ResolveNodeAsync(client, remoteDir)
                ?? throw new InvalidOperationException($"Remote directory not found: {remoteDir}");
        }

        if (progress != null && content.CanSeek)
        {
            var totalBytes = content.Length;
            var buffer = new byte[81920];
            var memStream = new MemoryStream();
            long bytesRead = 0;
            int read;
            while ((read = await content.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await memStream.WriteAsync(buffer, 0, read, ct);
                bytesRead += read;
                progress.Report(bytesRead * 1.0 / totalBytes);
            }
            memStream.Position = 0;
            await client.UploadAsync(memStream, fileName, parent);
        }
        else
        {
            using var memStream = new MemoryStream();
            await content.CopyToAsync(memStream, ct);
            memStream.Position = 0;
            await client.UploadAsync(memStream, fileName, parent);
        }
    }

    public async Task DownloadAsync(string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to download files.");

        var client = _authService.GetClient();
        var allNodes = await client.GetNodesAsync();
        var node = FindNodeByPath(allNodes, remotePath);
        if (node == null || node.Type != NodeType.File)
            throw new InvalidOperationException("File not found.");

        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (progress != null)
        {
            using var stream = await client.DownloadAsync(node);
            var totalBytes = stream.Length;
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
            var buffer = new byte[81920];
            long bytesDownloaded = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                bytesDownloaded += bytesRead;
                progress.Report(bytesDownloaded * 1.0 / totalBytes);
            }
        }
        else
        {
            using var stream = await client.DownloadAsync(node);
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream, ct);
        }
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (!_authService.IsSignedIn)
            throw new InvalidOperationException("User must be signed in to delete files.");

        var client = _authService.GetClient();
        var allNodes = await client.GetNodesAsync();
        var node = FindNodeByPath(allNodes, path);
        if (node == null)
            throw new InvalidOperationException("Item not found.");

        await client.DeleteAsync(node, moveToTrash: true);
    }

    private static async Task<INode?> ResolveNodeAsync(MegaApiClient client, string path)
    {
        var allNodes = await client.GetNodesAsync();
        if (string.IsNullOrEmpty(path) || path == "/")
            return allNodes.SingleOrDefault(n => n.Type == NodeType.Root);

        var byId = allNodes.FirstOrDefault(n => n.Id == path);
        if (byId != null)
            return byId;

        return FindNodeByPath(allNodes, path);
    }

    private static INode? FindNodeByPath(IEnumerable<INode> allNodes, string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var root = allNodes.SingleOrDefault(n => n.Type == NodeType.Root);
        if (root == null) return null;

        INode? current = root;
        foreach (var segment in segments)
        {
            var children = allNodes.Where(n => n.ParentId == current.Id);
            current = children.FirstOrDefault(n =>
                n.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (current == null) return null;
        }
        return current;
    }
}
