using System;
using OpenTelemetry.Exporter;
using UBS.AM.Observability.Configuration;

namespace UBS.AM.Observability.Resolvers
{
    /// <summary>
    /// Lightweight resolver for OTLP endpoints and protocol selection.
    /// Behavior:
    /// - Development/Local environments -> HTTP/protobuf endpoints (default localhost:4318)
    /// - Otherwise -> in-cluster gRPC endpoint (default otel-collector:4317)
    /// </summary>
    internal static class OtelEndpointResolver
    {
        /// <summary>
        /// Gets the OTLP endpoint based on the environment.
        /// </summary>
        /// <param name="v1Path">The specific OTLP path (e.g., "v1/traces").</param>
        /// <returns>The resolved <see cref="Uri"/> for the OTLP endpoint.</returns>
        internal static Uri GetEndpoint(string v1Path)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                      ?? "Production";

            if (IsLocalOrDevEnvironment(env))
            {
                var ingressBase = OtlpEndpoints.HttpEndpoint;
                var full = ingressBase.TrimEnd('/') + "/" + v1Path.TrimStart('/');
                return new Uri(full);
            }

            // In-cluster default (gRPC)
            return new Uri(OtlpEndpoints.GrpcEndpoint);
        }

        /// <summary>
        /// Gets the OTLP protocol based on the environment.
        /// </summary>
        /// <returns>The <see cref="OtlpExportProtocol"/> for the current environment, or null if using the default.</returns>
        internal static OtlpExportProtocol? GetProtocol()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                      ?? "Production";

            if (IsLocalOrDevEnvironment(env))
            {
                return OtlpExportProtocol.HttpProtobuf;
            }

            return null;
        }

        /// <summary>
        /// Determines if the environment is a local or development environment.
        /// </summary>
        /// <param name="env">The environment string.</param>
        /// <returns>True if the environment is local or development; otherwise, false.</returns>
        private static bool IsLocalOrDevEnvironment(string env)
        {
            return env.Equals("Development", StringComparison.OrdinalIgnoreCase)
                || env.Equals("Dev", StringComparison.OrdinalIgnoreCase)
                || env.Equals("Local", StringComparison.OrdinalIgnoreCase);
        }
    }
}
