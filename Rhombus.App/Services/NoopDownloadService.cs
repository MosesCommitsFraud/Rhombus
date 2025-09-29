using Rhombus.Core.Download;

namespace Rhombus.App.Services;

public class NoopDownloadService : IDownloadService
{
    public Task<DownloadResult> DownloadAsync(DownloadRequest request, IProgress<string>? log = null, CancellationToken ct = default)
    {
        log?.Report($"[NOOP] Would download {request.Kind} from {request.SourceUrl} to {request.OutputDirectory}");
        // Return success without doing anything
        return Task.FromResult(new DownloadResult(true, null, null));
    }
}
