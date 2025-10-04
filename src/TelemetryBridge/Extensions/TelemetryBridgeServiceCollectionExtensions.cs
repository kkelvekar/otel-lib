using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TelemetryBridge.Internal.Configuration;
using TelemetryBridge.Internal.Diagnostics;
using TelemetryBridge.Internal.Logging;

namespace TelemetryBridge;

/// <summary>
/// Extension methods for wiring the Telemetry Bridge into an application.
/// </summary>
public static class TelemetryBridgeServiceCollectionExtensions
{
    private static int _propagatorInitialized;

    /// <summary>
    /// Registers OpenTelemetry tracing and metrics along with developer log mirroring.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="serviceName">The logical service name used for telemetry resources.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is empty.</exception>
    public static IServiceCollection AddTelemetryBridge(this IServiceCollection services, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name must be provided.", nameof(serviceName));
        }

        serviceName = serviceName.Trim();

        EnsurePropagator();

        var options = TelemetryBridgeOptions.Create(serviceName);

        services.TryAddSingleton(options);
        services.TryAddSingleton(_ => options.CreateActivitySource());
        services.TryAddSingleton(_ => options.CreateMeter());
        services.TryAddSingleton<IMetricsHandle, MetricsHandle>();
        services.TryAddSingleton<ITelemetryLogEventEmitter, TelemetryLogEventEmitter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, TraceScopeStartupFilter>());
        services.TryAddTransient<TraceScopeMiddleware>();
        services.TryAddTransient<TraceScopeHandler>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<HttpClientFactoryOptions>, TraceScopeHttpClientOptions>());

        services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TelemetryLogger<>)));

        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => TelemetryResourceBuilder.Configure(resourceBuilder, options))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource(options.ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // gRPC instrumentation packages remain prerelease on NuGet; rely on the ASP.NET Core/HTTP
                    // instrumentation until stable versions are published.
                    .SetSampler(new AlwaysOnSampler())
                    .AddOtlpExporter(options => OtlpExporterOptionsResolver.Configure(options));
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddMeter(options.ServiceName)
                    .AddRuntimeInstrumentation()
                    // Process instrumentation is only available via prerelease packages; omit it to keep dependencies stable.
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => OtlpExporterOptionsResolver.Configure(options));
            });

        return services;
    }

    private static void EnsurePropagator()
    {
        if (Interlocked.Exchange(ref _propagatorInitialized, 1) == 1)
        {
            return;
        }

        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
        {
            new TraceContextPropagator(),
            new BaggagePropagator(),
        }));
    }
}
