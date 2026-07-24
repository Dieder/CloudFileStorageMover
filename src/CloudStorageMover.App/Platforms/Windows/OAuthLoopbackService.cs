using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using CloudStorageMover.App.Services;

namespace CloudStorageMover.App.Platforms.Windows;

public class OAuthLoopbackService : IOAuthLoopbackService
{
    public async Task<OAuthLoopbackResult> StartAsync(string authUrl, CancellationToken ct = default)
    {
        var port = GetRandomUnusedPort();
        var redirectUri = $"http://localhost:{port}/";

        var modifiedAuthUrl = Regex.Replace(authUrl,
            @"redirect_uri=[^&]+",
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}");

        var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        try
        {
            await Launcher.OpenAsync(new Uri(modifiedAuthUrl));

            var getContextTask = listener.GetContextAsync();
            var completedTask = await Task.WhenAny(getContextTask, Task.Delay(-1, ct));

            if (completedTask == getContextTask)
            {
                var context = getContextTask.Result;
                var code = context.Request.QueryString["code"];
                var error = context.Request.QueryString["error"];

                var response = "<html><body><p>Authentication complete. You can close this window.</p></body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length, ct);
                context.Response.OutputStream.Close();

                return new OAuthLoopbackResult(code, error, redirectUri);
            }
            else
            {
                ct.ThrowIfCancellationRequested();
                return new OAuthLoopbackResult(null, "cancelled", redirectUri);
            }
        }
        finally
        {
            try { listener.Stop(); } catch { }
            try { listener.Close(); } catch { }
        }
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
