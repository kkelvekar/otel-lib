using System;
using System.Collections.Generic;
using OpenTelemetry.Resources;
using TelemetryBridge.Internal.Configuration;

namespace TelemetryBridge.Internal.Diagnostics;

/// <summary>
/// Builds consistent OpenTelemetry resources for the consuming application.
/// </summary>
internal static class TelemetryResourceBuilder
{
    public static void Configure(ResourceBuilder resourceBuilder, TelemetryBridgeOptions options)
    {
        resourceBuilder
            .AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion)
            .AddAttributes(new KeyValuePair<string, object?>[]
            {
                new("service.instance.id", Environment.MachineName),
            });
    }
}
