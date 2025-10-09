using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Rhombus.Core.Download;

public sealed record DownloadRequest(
    string SourceUrl,
    string OutputDirectory,
    DownloadKind Kind,
    string? TargetFormat = null
);

public enum DownloadKind { Audio, Video }

public sealed record DownloadResult(bool Success, string? OutputPath, string? Error);

public sealed class DownloadService
{
    private const string YtDlpExecutable = "yt-dlp";
    private readonly string _ffmpegPath;
    
    public DownloadService()
    {
        // Get the path to ffmpeg.exe in the application's bin directory
        var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;
        _ffmpegPath = Path.Combine(appDirectory, "ffmpeg.exe");
    }
    
    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request, 
        IProgress<string>? log = null, 
        CancellationToken ct = default)
    {
        try
        {
            // Ensure output directory exists
            if (!Directory.Exists(request.OutputDirectory))
            {
                Directory.CreateDirectory(request.OutputDirectory);
                log?.Report($"Created directory: {request.OutputDirectory}");
            }

            // Build yt-dlp arguments
            var arguments = BuildYtDlpArguments(request);
            log?.Report($"Executing: {YtDlpExecutable} {arguments}");

            // Execute yt-dlp
            var processResult = await ExecuteProcessAsync(
                YtDlpExecutable, 
                arguments, 
                request.OutputDirectory,
                log, 
                ct);

            if (!processResult.Success)
            {
                return new DownloadResult(false, null, processResult.Error);
            }

            // Find the downloaded file
            var outputPath = FindDownloadedFile(request.OutputDirectory, request.TargetFormat ?? "mp3");
            
            if (outputPath != null)
            {
                log?.Report($"Download completed: {outputPath}");
                return new DownloadResult(true, outputPath, null);
            }
            else
            {
                return new DownloadResult(false, null, "Downloaded file not found");
            }
        }
        catch (Exception ex)
        {
            log?.Report($"Error: {ex.Message}");
            return new DownloadResult(false, null, ex.Message);
        }
    }

    private string BuildYtDlpArguments(DownloadRequest request)
    {
        var args = new List<string>();

        // Extract audio if Kind is Audio
        if (request.Kind == DownloadKind.Audio)
        {
            args.Add("--extract-audio");
            
            // Set audio format (default to mp3)
            var format = request.TargetFormat ?? "mp3";
            args.Add($"--audio-format {format}");
        }
        else
        {
            // For video, optionally specify format
            if (!string.IsNullOrEmpty(request.TargetFormat))
            {
                args.Add($"--format \"bestvideo[ext={request.TargetFormat}]+bestaudio\" --merge-output-format {request.TargetFormat}");
            }
            else
            {
                args.Add("--format bestvideo+bestaudio");
            }
        }

        // Output template (save in the output directory with title as filename)
        args.Add($"--output \"{Path.Combine(request.OutputDirectory, "%(title)s.%(ext)s")}\"");

        // Specify ffmpeg location
        if (File.Exists(_ffmpegPath))
        {
            args.Add($"--ffmpeg-location \"{_ffmpegPath}\"");
        }

        // No playlist, single video only
        args.Add("--no-playlist");

        // Show progress
        args.Add("--progress");

        // Add the URL (must be last)
        args.Add($"\"{request.SourceUrl}\"");

        return string.Join(" ", args);
    }

    private async Task<(bool Success, string? Error)> ExecuteProcessAsync(
        string executable,
        string arguments,
        string workingDirectory,
        IProgress<string>? log,
        CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                log?.Report(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
                log?.Report($"[stderr] {e.Data}");
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            if (process.ExitCode == 0)
            {
                return (true, null);
            }
            else
            {
                var errorMessage = errorBuilder.Length > 0 
                    ? errorBuilder.ToString() 
                    : $"Process exited with code {process.ExitCode}";
                return (false, errorMessage);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Failed to execute {executable}: {ex.Message}");
        }
    }

    private string? FindDownloadedFile(string directory, string extension)
    {
        try
        {
            // Find the most recently created file with the specified extension
            var files = Directory.GetFiles(directory, $"*.{extension}")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToArray();

            return files.FirstOrDefault()?.FullName;
        }
        catch
        {
            return null;
        }
    }
}

