using System;
using Microsoft.Extensions.Logging;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Decorates the default logger to mirror developer log messages into the current activity.
/// </summary>
/// <typeparam name="T">The logger category type.</typeparam>
internal sealed class TelemetryLogger<T> : ILogger<T>
{
    private readonly ILogger _innerLogger;
    private readonly ITelemetryLogEventEmitter _emitter;
    private readonly string _categoryName;

    public TelemetryLogger(ILoggerFactory loggerFactory, ITelemetryLogEventEmitter emitter)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(emitter);

        _categoryName = typeof(T).FullName ?? typeof(T).Name;
        _innerLogger = loggerFactory.CreateLogger(_categoryName);
        _emitter = emitter;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => _innerLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);
        var message = formatter(state, exception);
        _emitter.Record(_categoryName, logLevel, eventId, state, exception, message);
        _innerLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
