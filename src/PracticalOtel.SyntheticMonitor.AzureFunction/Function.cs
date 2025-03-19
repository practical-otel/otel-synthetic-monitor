using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace PracticalOtel.SyntheticMonitor.AzureFunction;
public class Function(IOptions<MonitorSettings> settings, MonitorManager MonitorManager, ILoggerFactory loggerFactory, TracerProvider tracerProvider)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    [Function("Function")]
    public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Running checks at: {DateTime.Now}");

        var urls = string.IsNullOrEmpty(settings.Value.Urls) ? [] : settings.Value.Urls.Split(',');
        var ips = string.IsNullOrEmpty(settings.Value.Ips) ? [] : settings.Value.Ips.Split(',');

        await MonitorManager.MonitorAllAsync(urls, ips);

        tracerProvider.ForceFlush();
    }
}