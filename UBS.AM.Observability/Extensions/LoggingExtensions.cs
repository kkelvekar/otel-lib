using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using System;
using UBS.AM.Observability.Resolvers;

namespace UBS.AM.Observability
{
    /// <summary>
    /// Extension methods for configuring logging.
    /// </summary>
    internal static class LoggingExtensions
    {
        /// <summary>
        /// Adds logging configuration for UBS Advanced Observability.
        /// </summary>
        /// <param name="loggingProviderBuilder">The <see cref="LoggerProviderBuilder"/> to configure.</param>
        /// <param name="endpoint">The OTLP endpoint <see cref="Uri"/>.</param>
        /// <param name="protocol">The OTLP export protocol.</param>
        /// <returns>The <see cref="LoggerProviderBuilder"/> so that additional calls can be chained.</returns>
        internal static LoggerProviderBuilder AddLogging(this LoggerProviderBuilder loggingProviderBuilder, Uri? endpoint = null, OtlpExportProtocol? protocol = null)
        {
            return loggingProviderBuilder.AddOtlpExporter(options =>
            {
                options.Endpoint = endpoint is null
                    ? OtelEndpointResolver.GetEndpoint("v1/logs")
                    : new Uri($"{endpoint.ToString().TrimEnd('/')}/v1/logs");
                
                if (protocol.HasValue)
                {
                    options.Protocol = protocol.Value;
                }
            });
        }
    }
}
