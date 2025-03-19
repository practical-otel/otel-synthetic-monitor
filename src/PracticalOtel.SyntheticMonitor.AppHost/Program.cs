using Projects;

string[] ips = ["8.8.8.8"];
string[] urls = ["https://www.google.com"];

var builder = DistributedApplication.CreateBuilder(args);

var environmentVariables = new Dictionary<string, string>
{
    { "MonitorSettings:Urls", string.Join(",", urls) },
    { "MonitorSettings:IPs", string.Join(",", ips) }
};

var service = builder.AddAzureFunctionsProject<PracticalOtel_SyntheticMonitor_AzureFunction>("Azure-Function")
    .WithExplicitStart();
var function = builder.AddProject<PracticalOtel_SyntheticMonitor_Service>("Container-Service")
    .WithEnvironment("MonitorSettings:Interval", "5")
    .WithExplicitStart();

foreach (var environmentVariable in environmentVariables)
{
    service.WithEnvironment(environmentVariable.Key, environmentVariable.Value);
    function.WithEnvironment(environmentVariable.Key, environmentVariable.Value);
}

builder.Build().Run();
