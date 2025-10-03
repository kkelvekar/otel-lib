using Microsoft.Extensions.DependencyInjection;
using TelemetryBridge;
using Xunit;

namespace TelemetryBridge.Tests;

public sealed class AddTelemetryBridgeTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void AddTelemetryBridge_RejectsBlankServiceName(string? serviceName)
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.AddTelemetryBridge(serviceName!));
    }
}
