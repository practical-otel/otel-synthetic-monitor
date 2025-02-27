FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

# Set the working directory
WORKDIR /sln

# Copy the project file and restore the dependencies
COPY src/PracticalOtel.SyntheticMonitor/PracticalOtel.SyntheticMonitor.csproj src/PracticalOtel.SyntheticMonitor/
COPY src/PracticalOtel.SyntheticMonitor.Service/PracticalOtel.SyntheticMonitor.Service.csproj src/PracticalOtel.SyntheticMonitor.Service/
COPY src/PracticalOtel.SyntheticMonitor.AzureFunction/PracticalOtel.SyntheticMonitor.AzureFunction.csproj src/PracticalOtel.SyntheticMonitor.AzureFunction/
COPY otel-ping-monitor.sln .
RUN dotnet restore

COPY src/PracticalOtel.SyntheticMonitor src/PracticalOtel.SyntheticMonitor
COPY src/PracticalOtel.SyntheticMonitor.Service src/PracticalOtel.SyntheticMonitor.Service

# Build the application
RUN dotnet publish src/PracticalOtel.SyntheticMonitor.Service/PracticalOtel.SyntheticMonitor.Service.csproj \
    -c Release \
    -o /app \ 
    --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0.2 as publish

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "PracticalOtel.SyntheticMonitor.Service.dll"]