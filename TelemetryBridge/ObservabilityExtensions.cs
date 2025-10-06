using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using System;
using UBS.AM.Observability.Enums;

namespace UBS.AM.Observability
{
    /// <summary>
    /// Extension methods for setting up UBS Advanced Observability.
    /// </summary>
    public static class ObservabilityExtensions
    {
        /// <summary>
        /// Adds UBS Advanced Observability to the service collection with default endpoint resolution.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null or whitespace.</exception>
        public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName))
                .WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddTracing(serviceName))
                .WithMetrics(meterProviderBuilder => meterProviderBuilder.AddMetrics(serviceName))
                .WithLogging(loggingProviderBuilder => loggingProviderBuilder.AddLogging());

            return services;
        }

        /// <summary>
        /// Adds UBS Advanced Observability to the service collection with a specified endpoint and protocol.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="endpoint">The OTLP endpoint <see cref="Uri"/>.</param>
        /// <param name="protocol">The OTLP protocol to use.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null or whitespace.</exception>
        public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName, Uri endpoint, OtlpProtocol protocol)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            var otlpExportProtocol = ToOtlpExportProtocol(protocol);

            services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName))
                .WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddTracing(serviceName, endpoint, otlpExportProtocol))
                .WithMetrics(meterProviderBuilder => meterProviderBuilder.AddMetrics(serviceName, endpoint, otlpExportProtocol))
                .WithLogging(loggingProviderBuilder => loggingProviderBuilder.AddLogging(endpoint, otlpExportProtocol));

            return services;
        }

        /// <summary>
        /// Converts the custom <see cref="OtlpProtocol"/> to the official <see cref="OtlpExportProtocol"/>.
        /// </summary>
        /// <param name="protocol">The custom OTLP protocol.</param>
        /// <returns>The corresponding <see cref="OtlpExportProtocol"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the protocol is not supported.</exception>
        private static OtlpExportProtocol ToOtlpExportProtocol(OtlpProtocol protocol)
        {
            return protocol switch
            {
                OtlpProtocol.Grpc => OtlpExportProtocol.Grpc,
                OtlpProtocol.HttpProtobuf => OtlpExportProtocol.HttpProtobuf,
                _ => throw new ArgumentOutOfRangeException(nameof(protocol), $"Not supported protocol: {protocol}"),
            };
        }
    }
}
