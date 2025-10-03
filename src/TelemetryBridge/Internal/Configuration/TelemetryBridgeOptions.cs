using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace TelemetryBridge.Internal.Configuration;

/// <summary>
/// Configuration container that keeps shared telemetry settings.
/// </summary>
internal sealed class TelemetryBridgeOptions
{
    private TelemetryBridgeOptions(string serviceName, string serviceVersion, string developerLogCategoryPrefix)
    {
        ServiceName = serviceName;
        ServiceVersion = serviceVersion;
        DeveloperLogCategoryPrefix = developerLogCategoryPrefix;
    }

    public string ServiceName { get; }

    public string ServiceVersion { get; }

    public string DeveloperLogCategoryPrefix { get; }

    public static TelemetryBridgeOptions Create(string serviceName)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var version = entryAssembly?.GetName().Version?.ToString() ?? typeof(TelemetryBridgeOptions).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        return new TelemetryBridgeOptions(serviceName, version, developerLogCategoryPrefix: serviceName);
    }

    public ActivitySource CreateActivitySource() => new(ServiceName);

    public Meter CreateMeter() => new(ServiceName);
}
