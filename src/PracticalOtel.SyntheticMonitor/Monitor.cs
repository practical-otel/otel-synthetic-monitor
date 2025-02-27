using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace PracticalOtel.SyntheticMonitor;

public class Monitor(ILogger<Monitor> logger, HttpClient httpClient)
{
    private readonly ILogger<Monitor> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ActivityContext> MonitorUrlAsync(Uri uri)
    {
        using var activity = DiagnosticConfig.Source.StartActivity($"UrlMonitor {uri.OriginalString}");
        activity?.SetTag("sm.monitored_url.full", uri.OriginalString);
        activity?.SetTag("sm.monitored_url.scheme", uri.Scheme);
        activity?.SetTag("sm.monitored_url.host", uri.Host);
        activity?.SetTag("sm.monitored_url.port", uri.Port.ToString());
        activity?.SetTag("sm.monitored_url.path", uri.AbsolutePath);
        activity?.SetTag("sm.monitored_url.query", uri.Query);

        _logger.LogInformation("Monitoring {url}", uri);

        var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        activity?.SetTag("http.response.code", response.StatusCode.ToString());
        activity?.SetTag("http.response.size", response.Content.Headers.ContentLength?.ToString() ?? "0");
        activity?.SetTag("http.response.type", response.Content.Headers.ContentType?.ToString() ?? "unknown");
        activity?.SetTag("http.response.length", response.Content.Headers.ContentLength?.ToString() ?? "0");
        activity?.SetTag("http.response.last_modified" ,response.Content.Headers.LastModified);
        using var contentActivity = DiagnosticConfig.Source.StartActivity("ReadContent");
        await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Ping to {url} succeeded", uri);
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            _logger.LogError("Ping to {url} failed", uri);
        }

        return activity?.Context ?? default;
    }

    public async Task<ActivityContext> MonitorIpAsync(string name, string ip)
    {
        using var activity = DiagnosticConfig.Source.StartActivity($"IpMonitor {name}");
        activity?.SetTag("sm.monitored_ip", ip);

        _logger.LogInformation("Monitoring {ip}", ip);

        var ping = new Ping();
        var reply = await ping.SendPingAsync(ip);
        activity?.SetTag("ping.response.status", reply.Status.ToString());
        activity?.SetTag("ping.response.time", reply.RoundtripTime.ToString());

        return activity?.Context ?? default;
    }
}


public class MonitorManager
{
    private readonly Monitor _monitor;
    private readonly ILogger<MonitorManager> _logger;

    public MonitorManager(Monitor monitor, ILogger<MonitorManager> logger)
    {
        _monitor = monitor;
        _logger = logger;
    }

    public async Task MonitorAllAsync(IEnumerable<string> urls, IEnumerable<string> ips)
    {
        _logger.LogInformation("Monitoring {urls} and {ips}", urls, ips);
        using var activity = DiagnosticConfig.Source.StartActivity("StartMonitoring");
        activity?.SetTag("sm.monitored_urls.count", urls.Count().ToString());
        activity?.SetTag("sm.monitored_ips.count", ips.Count().ToString());

        var tasks = new List<Tuple<string, Task<ActivityContext>>>();

        // Hack to reset the parent so that the monitors start a new span
        Activity.Current = null;

        foreach (var url in urls)
        {
            tasks.Add(new(url, _monitor.MonitorUrlAsync(new Uri(url))));
        }

        foreach (var ip in ips)
        {
            tasks.Add(new(ip, _monitor.MonitorIpAsync(ip, ip)));
        }

        await Task.WhenAll(tasks.Select(t => t.Item2));

        tasks
            .ForEach(task => {
                activity?.AddLink(new ActivityLink(task.Item2.Result, new ActivityTagsCollection() {
                    { "sm.monitored_url",  task.Item1 }
                }));
            });
    }
}