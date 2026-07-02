using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileMover.UI.Services;

public class WifiTransferService
{
    private TcpListener? listener;
    private CancellationTokenSource? listenCts;

    public event EventHandler<TransferProgressArgs>? ProgressUpdated;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler? TransferCompleted;

    public async Task StartServerAsync(int port, string saveDirectory, CancellationToken cancellationToken = default)
    {
        listenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        try
        {
            while (!listenCts.Token.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(listenCts.Token);
                _ = HandleClientAsync(client, saveDirectory, listenCts.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            listener.Stop();
        }
    }

    public async Task SendFilesAsync(string serverAddress, int port, List<string> filePaths, CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(serverAddress, port, cancellationToken);

        using var stream = client.GetStream();
        int totalFiles = filePaths.Count;
        int completedFiles = 0;

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) continue;

            // Send file header: filename|filesize
            var header = $"{fileInfo.Name}|{fileInfo.Length}\n";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            await stream.WriteAsync(headerBytes, cancellationToken);

            // Send file content
            using var fileStream = fileInfo.OpenRead();
            var buffer = new byte[81920];
            long totalBytes = 0;
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytes += bytesRead;

                ProgressUpdated?.Invoke(this, new TransferProgressArgs
                {
                    CurrentFile = fileInfo.Name,
                    FileProgress = (double)totalBytes / fileInfo.Length,
                    OverallProgress = (double)(completedFiles + (double)totalBytes / fileInfo.Length) / totalFiles,
                    CompletedFiles = completedFiles,
                    TotalFiles = totalFiles
                });
            }

            completedFiles++;
        }

        TransferCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        listenCts?.Cancel();
        listener?.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, string saveDirectory, CancellationToken cancellationToken)
    {
        using (client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[81920];

            while (!cancellationToken.IsCancellationRequested)
            {
                // Read header
                var headerBuilder = new StringBuilder();
                int b;
                while ((b = stream.ReadByte()) != '\n' && b != -1)
                {
                    headerBuilder.Append((char)b);
                }

                if (b == -1) break;

                var header = headerBuilder.ToString();
                var parts = header.Split('|');
                if (parts.Length < 2) continue;

                var fileName = parts[0];
                var fileSize = long.Parse(parts[1]);

                var savePath = Path.Combine(saveDirectory, fileName);
                using var fileStream = File.Create(savePath);
                long totalRead = 0;
                int bytesRead;

                while (totalRead < fileSize && (bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;
                }
            }
        }
    }
}

public class TransferProgressArgs : EventArgs
{
    public string CurrentFile { get; set; } = string.Empty;
    public double FileProgress { get; set; }
    public double OverallProgress { get; set; }
    public int CompletedFiles { get; set; }
    public int TotalFiles { get; set; }
}
