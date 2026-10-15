using System.Diagnostics;
using Xunit;

namespace Escandallo.Tests.Browser;

public class AppHostFixture : IAsyncLifetime
{
    private Process? _apiProcess;
    private Process? _webProcess;

    public string ApiBaseUrl { get; } = "http://127.0.0.1:5000";
    public string WebBaseUrl { get; } = "http://127.0.0.1:5100";

    public async Task InitializeAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };

        // 1. Ensure API is running on port 5000
        if (!await IsServerRunningAsync(client, $"{ApiBaseUrl}/healthz"))
        {
            _apiProcess = StartProcess("src/Escandallo.Api", "src/Escandallo.Api/bin/Debug/net10.0/Escandallo.Api.dll", "--urls http://127.0.0.1:5000");
            await WaitForServerAsync(client, $"{ApiBaseUrl}/healthz", TimeSpan.FromSeconds(15));
        }

        // 2. Ensure Web is running on port 5100
        if (!await IsServerRunningAsync(client, WebBaseUrl))
        {
            _webProcess = StartProcess("src/Escandallo.Web", "src/Escandallo.Web/bin/Debug/net10.0/Escandallo.Web.dll", "--urls http://127.0.0.1:5100");
            await WaitForServerAsync(client, WebBaseUrl, TimeSpan.FromSeconds(15));
        }
    }

    private static Process StartProcess(string workingDir, string dllPath, string args)
    {
        string rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
        string fullWorkingDir = Path.Combine(rootDir, workingDir);
        string fullDllPath = Path.Combine(rootDir, dllPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{fullDllPath}\" {args}",
            WorkingDirectory = fullWorkingDir,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(startInfo);
        if (process == null || process.HasExited)
        {
            throw new InvalidOperationException($"Failed to start process for {workingDir}");
        }
        return process;
    }

    private static async Task<bool> IsServerRunningAsync(HttpClient client, string url)
    {
        try
        {
            var res = await client.GetAsync(url);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task WaitForServerAsync(HttpClient client, string url, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (await IsServerRunningAsync(client, url)) return;
            await Task.Delay(300);
        }
        throw new TimeoutException($"Server at {url} did not respond within {timeout.TotalSeconds}s");
    }

    public Task DisposeAsync()
    {
        try { _webProcess?.Kill(entireProcessTree: true); } catch { }
        try { _apiProcess?.Kill(entireProcessTree: true); } catch { }
        return Task.CompletedTask;
    }
}
