using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using System;
using UBS.AM.Observability.Resolvers;

namespace UBS.AM.Observability
{
    /// <summary>
    /// Extension methods for configuring metrics.
    /// </summary>
    internal static class MetricsExtensions
    {
        /// <summary>
        /// Adds metrics configuration for UBS Advanced Observability.
        /// </summary>
        /// <param name="meterProviderBuilder">The <see cref="MeterProviderBuilder"/> to configure.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="endpoint">The OTLP endpoint <see cref="Uri"/>.</param>
        /// <param name="protocol">The OTLP export protocol.</param>
        /// <returns>The <see cref="MeterProviderBuilder"/> so that additional calls can be chained.</returns>
        internal static MeterProviderBuilder AddMetrics(this MeterProviderBuilder meterProviderBuilder, string serviceName, Uri endpoint = null, OtlpExportProtocol? protocol = null)
        {
            return meterProviderBuilder
                .AddMeter(serviceName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(options =>
                {
                    var metricsEndpoint = endpoint is null ? OtelEndpointResolver.GetEndpoint("v1/metrics") : new Uri(endpoint.ToString().TrimEnd('/') + "/v1/metrics");
                    options.Endpoint = metricsEndpoint;
                    var proto = protocol ?? OtelEndpointResolver.GetProtocol();
                    if (proto.HasValue) options.Protocol = proto.Value;
                });
        }
    }
}
