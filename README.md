# Synthetic Monitor using OpenTelemetry

This is a fairly basic ping and URL monitor that uses OpenTelemetry, specifically Spans, to output the result.

The idea is that it will run on a schedule and use an ICMP ping for IP Addresses, and a HTTP Client for URLs.

## Deployment

There is an Azure Function that will run on a Timer trigger, and also a Docker image you can use that's long running.

The data is exported using the standard OpenTelemetry libraries, so you can use the OpenTelemetry Environment Variables to configure the exporting. More information on the Environment variables is [Here](https://opentelemetry.io/docs/specs/otel/configuration/sdk-environment-variables/).

## Configuration

For the Console Application you can use an `appsettings.Production.json`. There's an example in the `appsettings.json`

For both the Azure Function and the Console application, you can use Environment Variables

```shell
export MonitorSettings__Urls=https://bing.com,https://google.co.uk
export MonitorSettings__Ips=8.8.8.8,1.1.1.1
export MonitorSettings__Interval=10 # seconds
```

## Build

To run the Console app

```shell
cd src/PracticalOtel.SyntheticMonitor.Service
dotnet run
```

to run the Azure Function

```shell
cd src/PracticalOtel.SyntheticMonitor.AzureFunction
func start
```

To build and run the docker image

```shell
docker build -t local/otel-ping-monitor
docker run -e MonitorSettings__Url=https://www.google.co.uk local/otel-ping-monitor
```

# .NET Aspire

This solution is also setup to use .NET Aspire. Both the Azure Function and the Console app are available, however, neither of them start by default, you need to use the start button in the resources tab to start each of them.