using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelemetryBridge;
using TestApp;
using Xunit;

namespace TelemetryBridge.Tests;

public sealed class TelemetryLoggerTests
{
    [Fact]
    public void Logger_AddsSpanEvent_WithDeveloperLogs()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTelemetryBridge("TestApp");

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<SampleLoggerCategory>>();

        using var activity = new Activity("test");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        var original = Activity.Current;
        activity.Start();
        Activity.Current = activity;

        var exception = new InvalidOperationException("boom");
        logger.LogError(exception, "Failure for {Id}", 42);

        activity.Stop();
        Activity.Current = original;

        var logEvent = Assert.Single(activity.Events);
        Assert.Equal("log", logEvent.Name);
        Assert.Contains(logEvent.Tags, tag => tag.Key == "log.severity" && Equals(tag.Value, LogLevel.Error.ToString()));
        Assert.Contains(logEvent.Tags, tag => tag.Key == "log.category" && Equals(tag.Value, typeof(SampleLoggerCategory).FullName));
        Assert.Contains(logEvent.Tags, tag => tag.Key == "exception.message" && Equals(tag.Value, exception.Message));
    }

}
