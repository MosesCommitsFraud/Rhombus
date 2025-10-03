using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rhombus.App.Services;
using Rhombus.App.ViewModels;
using Rhombus.Core.Download;

namespace Rhombus.App;

public class Program
{
    [STAThread] // Single-threaded apartment für WPF sachen
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(l =>
            {
                l.ClearProviders();
                l.AddConsole();
            })
            .ConfigureServices(services =>
            {
                // Services
                services.AddSingleton<IDownloadService>(sp => new StubDownloadService());
                services.AddSingleton<AudioPlaybackService>();
                services.AddSingleton<GlobalHotkeyService>();
                services.AddSingleton<SettingsService>();

                // ViewModels
                services.AddSingleton<SoundboardViewModel>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

        var app = new App(host);
        app.InitializeComponent();
        app.Run(host.Services.GetRequiredService<MainWindow>());
    }
}

// Temporary stub until real implementation is added
internal class StubDownloadService : IDownloadService
{
    public Task<DownloadResult> DownloadAsync(DownloadRequest request, IProgress<string>? log = null, CancellationToken ct = default)
    {
        log?.Report("Download service not yet implemented.");
        return Task.FromResult(new DownloadResult(false, null, "Download service not yet implemented."));
    }
}
