using System.Diagnostics;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using PracticalOtel.SyntheticMonitor;
using Monitor = PracticalOtel.SyntheticMonitor.Monitor;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MonitorManager>();
builder.Services.AddSingleton<Monitor>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("azurefunction-monitor"))
    .UseOtlpExporter()
    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
        .AddSource(DiagnosticConfig.Source.Name)
        .AddSource(PracticalOtel.SyntheticMonitor.DiagnosticConfig.Source.Name)
    );

builder.Services.Configure<MonitorSettings>(builder.Configuration.GetSection("MonitorSettings"));

builder.Build().Run();

public class MonitorSettings
{
    public string Urls { get; set; } = null!;
    public string Ips { get; set; } = null!;
}

internal class DiagnosticConfig
{
    public static readonly ActivitySource Source = new("PracticalOtel.SyntheticMonitor.AzureFunction");
}