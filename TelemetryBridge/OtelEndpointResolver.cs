using System;
using OpenTelemetry.Exporter;

namespace TelemetryBridge
{
  /// <summary>
  /// Lightweight resolver for OTLP endpoints and protocol selection.
  /// Behavior:
  /// - Development/Local environments -> HTTP/protobuf endpoints (default localhost:4318)
  /// - Otherwise -> in-cluster gRPC endpoint (default otel-collector:4317)
  /// </summary>
  public static class OtelEndpointResolver
  {
    // v1Path examples: "v1/traces", "v1/metrics", "v1/logs"
    public static Uri GetEndpoint(string v1Path)
    {
      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Production";

      if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
          env.Equals("Local", StringComparison.OrdinalIgnoreCase))
      {
        var ingressBase = Environment.GetEnvironmentVariable("OTEL_COLLECTOR_INGRESS_URL")
                          ?? "http://localhost:4318";
        var full = ingressBase.TrimEnd('/') + "/" + v1Path.TrimStart('/');
        return new Uri(full);
      }

      // In-cluster default (gRPC)
      return new Uri("http://otel-collector:4317");
    }

    // Returns HttpProtobuf for dev/local, otherwise null (leave exporter default -> gRPC)
    public static OtlpExportProtocol? GetProtocol()
    {
      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Production";

      if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
          env.Equals("Local", StringComparison.OrdinalIgnoreCase))
      {
        return OtlpExportProtocol.HttpProtobuf;
      }

      return null;
    }
  }
}
