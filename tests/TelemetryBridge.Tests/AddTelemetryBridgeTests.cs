using Microsoft.Extensions.DependencyInjection;
using TelemetryBridge;
using Xunit;

namespace TelemetryBridge.Tests;

internal sealed class AddTelemetryBridgeTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void AddTelemetryBridgeRejectsBlankServiceName(string? serviceName)
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.AddTelemetryBridge(serviceName!));
    }
}
