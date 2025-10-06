using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using System;
using UBS.AM.Observability.Resolvers;

namespace UBS.AM.Observability
{
    /// <summary>
    /// Extension methods for configuring tracing.
    /// </summary>
    internal static class TracingExtensions
    {
        /// <summary>
        /// Adds tracing configuration for UBS Advanced Observability.
        /// </summary>
        /// <param name="tracerProviderBuilder">The <see cref="TracerProviderBuilder"/> to configure.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="endpoint">The OTLP endpoint <see cref="Uri"/>.</param>
        /// <param name="protocol">The OTLP export protocol.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> so that additional calls can be chained.</returns>
        internal static TracerProviderBuilder AddTracing(this TracerProviderBuilder tracerProviderBuilder, string serviceName, Uri endpoint = null, OtlpExportProtocol? protocol = null)
        {
            return tracerProviderBuilder
                .AddSource(serviceName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    var tracesEndpoint = endpoint is null ? OtelEndpointResolver.GetEndpoint("v1/traces") : new Uri(endpoint.ToString().TrimEnd('/') + "/v1/traces");
                    options.Endpoint = tracesEndpoint;
                    var proto = protocol ?? OtelEndpointResolver.GetProtocol();
                    if (proto.HasValue) options.Protocol = proto.Value;
                });
        }
    }
}
