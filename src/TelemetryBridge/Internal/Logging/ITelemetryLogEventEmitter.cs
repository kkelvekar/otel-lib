using Microsoft.Extensions.Logging;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Provides a hook for mirroring developer log messages into the active <see cref="System.Diagnostics.Activity"/>.
/// </summary>
internal interface ITelemetryLogEventEmitter
{
    void Record<TState>(string category, LogLevel level, EventId eventId, TState state, Exception? exception, string message);
}
