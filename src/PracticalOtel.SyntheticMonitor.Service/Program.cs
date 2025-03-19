using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using PracticalOtel.SyntheticMonitor;
using Monitor = PracticalOtel.SyntheticMonitor.Monitor;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<MonitorWorkerSettings>(hostContext.Configuration.GetSection("MonitorSettings"));
        services.AddSingleton<MonitorManager>();
        services.AddSingleton<Monitor>();
        services.AddHttpClient();
        services.AddHostedService<MonitorWorker>();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("PracticalOtel.SyntheticMonitor.Service"))
            .UseOtlpExporter()
            .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddSource(DiagnosticConfig.Source.Name)
            );
    });

await hostBuilder.RunConsoleAsync();

public class MonitorWorker(MonitorManager monitorManager, IOptions<MonitorWorkerSettings> settings) : BackgroundService
{
    private readonly MonitorManager _monitorManager = monitorManager;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var urls = string.IsNullOrEmpty(settings.Value.Urls) ? [] : settings.Value.Urls.Split(',');
            var ips = string.IsNullOrEmpty(settings.Value.Ips) ? [] : settings.Value.Ips.Split(',');
            await _monitorManager.MonitorAllAsync(urls, ips);
            await Task.Delay(settings.Value.Interval * 1000, stoppingToken);
        }
    }
}

public class MonitorWorkerSettings
{
    public int Interval { get; set; } = 300;
    public string Urls { get; set; } = null!;
    public string Ips { get; set; } = null!;
}