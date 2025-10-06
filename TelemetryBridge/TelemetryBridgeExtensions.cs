
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
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
                      .AddOtlpExporter(options =>
                      {
                        var endpoint = OtelEndpointResolver.GetEndpoint("v1/traces");
                        options.Endpoint = endpoint;
                        var proto = OtelEndpointResolver.GetProtocol();
                        if (proto.HasValue) options.Protocol = proto.Value;
                      });
          })
          .WithMetrics(meterProviderBuilder =>
          {
            meterProviderBuilder
                      .AddMeter(serviceName)
                      .AddHttpClientInstrumentation()
                      .AddAspNetCoreInstrumentation()
                      .AddOtlpExporter(options =>
                      {
                        var endpoint = OtelEndpointResolver.GetEndpoint("v1/metrics");
                        options.Endpoint = endpoint;
                        var proto = OtelEndpointResolver.GetProtocol();
                        if (proto.HasValue) options.Protocol = proto.Value;
                      });
          })
          .WithLogging(loggingProviderBuilder =>
          {
            loggingProviderBuilder.AddOtlpExporter(options =>
                  {
                    var endpoint = OtelEndpointResolver.GetEndpoint("v1/logs");
                    options.Endpoint = endpoint;
                    var proto = OtelEndpointResolver.GetProtocol();
                    if (proto.HasValue) options.Protocol = proto.Value;
                  });
          });

      return services;
    }
  }
}
