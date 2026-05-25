using System.Diagnostics;

namespace MonoGame.Editor.Core.Assets;

/// <summary>Invokes the MonoGame Content Builder (MGCB) and streams its output.</summary>
public static class MgcbRunner
{
    /// <summary>
    /// Runs <c>dotnet mgcb</c> against <paramref name="mgcbFilePath"/> and streams each
    /// output line to <paramref name="onOutput"/>. Returns the process exit code.
    /// </summary>
    /// <param name="mgcbFilePath">Absolute path to the <c>.mgcb</c> file.</param>
    /// <param name="onOutput">Callback invoked for each stdout/stderr line.</param>
    /// <param name="cancellationToken">Token to cancel the build.</param>
    public static async Task<int> RunAsync(
        string mgcbFilePath,
        Action<string> onOutput,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mgcbFilePath);
        ArgumentNullException.ThrowIfNull(onOutput);

        string workingDir = Path.GetDirectoryName(mgcbFilePath) ?? string.Empty;

        ProcessStartInfo psi = new("dotnet", $"mgcb \"{mgcbFilePath}\"")
        {
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
            WorkingDirectory       = workingDir,
        };

        using Process process = new() { StartInfo = psi, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) onOutput(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) onOutput($"[ERR] {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        return process.ExitCode;
    }
}
