using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using TelemetryBridge;

namespace TelemetryBridge.Internal.Diagnostics;

/// <summary>
/// Default implementation of <see cref="IMetricsHandle"/> that caches instrument instances.
/// </summary>
internal sealed class MetricsHandle : IMetricsHandle, IDisposable
{
    private readonly ConcurrentDictionary<string, Instrument> _instruments = new(StringComparer.Ordinal);
    private bool _disposed;

    public MetricsHandle(Meter meter)
    {
        Meter = meter ?? throw new ArgumentNullException(nameof(meter));
    }

    public Meter Meter { get; }

    public Counter<long> CreateCounter(string name, string? unit = null, string? description = null)
    {
        EnsureNotDisposed();
        return (Counter<long>)_instruments.GetOrAdd(name, _ => Meter.CreateCounter<long>(name, unit, description));
    }

    public Histogram<double> CreateHistogram(string name, string? unit = null, string? description = null)
    {
        EnsureNotDisposed();
        return (Histogram<double>)_instruments.GetOrAdd(name, _ => Meter.CreateHistogram<double>(name, unit, description));
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetricsHandle));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var instrument in _instruments.Values)
        {
            instrument.Dispose();
        }

        _disposed = true;
    }
}
