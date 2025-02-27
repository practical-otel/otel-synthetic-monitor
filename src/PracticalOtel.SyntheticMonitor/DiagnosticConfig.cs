using System.Diagnostics;

namespace PracticalOtel.SyntheticMonitor;

public static class DiagnosticConfig
{
    public static readonly ActivitySource Source = new("PracticalOtel.PingMonitor");
}
