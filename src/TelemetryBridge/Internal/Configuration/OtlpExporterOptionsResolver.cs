using System;
using OpenTelemetry.Exporter;

namespace TelemetryBridge.Internal.Configuration;

/// <summary>
/// Resolves OTLP exporter configuration from environment variables.
/// </summary>
internal static class OtlpExporterOptionsResolver
{
    private const string EndpointVariable = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string ProtocolVariable = "OTEL_EXPORTER_OTLP_PROTOCOL";

    public static void Configure(OtlpExporterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var endpointText = Environment.GetEnvironmentVariable(EndpointVariable);
        if (!string.IsNullOrWhiteSpace(endpointText) && Uri.TryCreate(endpointText, UriKind.Absolute, out var endpoint))
        {
            options.Endpoint = endpoint;
        }
        else
        {
            options.Endpoint = new Uri("http://localhost:4317", UriKind.Absolute);
        }

        var protocolText = Environment.GetEnvironmentVariable(ProtocolVariable);
        if (!string.IsNullOrWhiteSpace(protocolText) && Enum.TryParse(protocolText, ignoreCase: true, out OtlpExportProtocol protocol))
        {
            options.Protocol = protocol;
        }
        else
        {
            options.Protocol = OtlpExportProtocol.Grpc;
        }
    }
}
