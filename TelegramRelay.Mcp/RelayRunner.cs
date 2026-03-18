namespace TelegramRelay.Mcp;

/// <summary>
/// Runs TelegramNotifier.exe and parses output for MCP.
/// Only one run at a time to avoid WTelegram.session file lock (each process opens the same session file).
/// </summary>
internal static class RelayRunner
{
    private static readonly SemaphoreSlim SingleRun = new(1, 1);

    private static string GetRelayExePath()
    {
        var fromEnv = Environment.GetEnvironmentVariable("TELEGRAM_RELAY_EXE");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
            return fromEnv;
        var nextToMe = Path.Combine(AppContext.BaseDirectory, "TelegramNotifier.exe");
        if (File.Exists(nextToMe))
            return nextToMe;
        throw new InvalidOperationException(
            "TelegramNotifier.exe not found. Set TELEGRAM_RELAY_EXE to the path of TelegramNotifier.exe, or place it next to TelegramRelay.Mcp.");
    }

    /// <summary>
    /// Run relay with args; returns (stdout, stderr, exitCode). Working directory = directory of the exe.
    /// Serialized so only one Notifier process runs at a time (avoids WTelegram.session lock).
    /// </summary>
    public static async Task<(string StdOut, string StdErr, int ExitCode)> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
    {
        await SingleRun.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var exe = GetRelayExePath();
            var workingDir = Path.GetDirectoryName(exe)!;
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe,
                Arguments = string.Join(" ", args.Select(a => a.Contains(' ') ? "\"" + a.Replace("\"", "\\\"") + "\"" : a)),
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException("Failed to start TelegramNotifier.exe");
            var outTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var stdout = await outTask.ConfigureAwait(false);
            var stderr = await errTask.ConfigureAwait(false);
            return (stdout ?? "", stderr ?? "", process.ExitCode);
        }
        finally
        {
            SingleRun.Release();
        }
    }

    /// <summary>
    /// Get the last line that looks like JSON array (for get-messages: login line may appear before).
    /// </summary>
    public static string? ExtractJsonLine(string stdout)
    {
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
                return line;
        }
        return null;
    }
}
