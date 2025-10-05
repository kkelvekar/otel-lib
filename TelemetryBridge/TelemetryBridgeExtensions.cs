
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using System;

namespace TelemetryBridge
{
    public static class TelemetryBridgeExtensions
    {
        public static IServiceCollection AddTelemetryBridge(this IServiceCollection services, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }



            services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName))
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource(serviceName)
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddGrpcClientInstrumentation()
                        .AddOtlpExporter();
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddMeter(serviceName)
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter();
                })
                .WithLogging(loggingProviderBuilder =>
                {
                    loggingProviderBuilder.AddOtlpExporter();
                });

            return services;
        }
    }
}
