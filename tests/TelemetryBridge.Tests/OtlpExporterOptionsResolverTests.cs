using System;
using System.Collections.Generic;
using OpenTelemetry.Exporter;
using TelemetryBridge.Internal.Configuration;
using Xunit;

namespace TelemetryBridge.Tests;

public sealed class OtlpExporterOptionsResolverTests : IDisposable
{
    private readonly Dictionary<string, string?> _originalEnvironment = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OTEL_EXPORTER_OTLP_ENDPOINT"] = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"),
        ["OTEL_EXPORTER_OTLP_PROTOCOL"] = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL"),
    };

    [Fact]
    public void Configure_UsesDefaults_WhenEnvVarsMissing()
    {
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", null);
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL", null);

        var options = new OtlpExporterOptions();
        OtlpExporterOptionsResolver.Configure(options);

        Assert.Equal(new Uri("http://localhost:4317"), options.Endpoint);
        Assert.Equal(OtlpExportProtocol.Grpc, options.Protocol);
    }

    [Fact]
    public void Configure_UsesEnvironmentOverrides()
    {
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://collector:4318");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf");

        var options = new OtlpExporterOptions();
        OtlpExporterOptionsResolver.Configure(options);

        Assert.Equal(new Uri("http://collector:4318"), options.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, options.Protocol);
    }

    public void Dispose()
    {
        foreach (var (key, value) in _originalEnvironment)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
