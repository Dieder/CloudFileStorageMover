namespace CloudStorageMover.Core.Services;

public static class AsyncFileOperations
{
    private const int BufferSize = 81920;

    public static async Task CopyAsync(
        string sourcePath,
        string destPath,
        IProgress<long>? progress = null,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path cannot be null or whitespace.", nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destPath))
            throw new ArgumentException("Destination path cannot be null or whitespace.", nameof(destPath));
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Source file not found.", sourcePath);

        var destDir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(destDir))
            Directory.CreateDirectory(destDir);

        if (File.Exists(destPath) && !overwrite)
            throw new IOException("Destination file exists and overwrite is false.");

        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);

        long totalBytesCopied = 0;
        var buffer = new byte[BufferSize];
        int bytesRead;

        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesCopied += bytesRead;
            progress?.Report(totalBytesCopied);
        }
    }

    public static async Task MoveAsync(
        string sourcePath,
        string destPath,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path cannot be null or whitespace.", nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destPath))
            throw new ArgumentException("Destination path cannot be null or whitespace.", nameof(destPath));
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Source file not found.", sourcePath);

        if (File.Exists(destPath) && !overwrite)
            throw new IOException("Destination file exists and overwrite is false.");

        bool isSameVolume = IsSameVolume(sourcePath, destPath);

        if (isSameVolume)
        {
            File.Move(sourcePath, destPath, overwrite);
        }
        else
        {
            await CopyAsync(sourcePath, destPath, overwrite: overwrite, cancellationToken: cancellationToken);
            File.Delete(sourcePath);
        }
    }

    private static bool IsSameVolume(string path1, string path2)
    {
        try
        {
            var drive1 = new DriveInfo(Path.GetFullPath(path1));
            var drive2 = new DriveInfo(Path.GetFullPath(path2));
            return drive1.Name.Equals(drive2.Name, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
