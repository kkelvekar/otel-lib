using System.Diagnostics.Metrics;

namespace TelemetryBridge;

/// <summary>
/// Exposes helper APIs for creating reusable metrics instruments tied to the application's meter.
/// </summary>
public interface IMetricsHandle
{
    /// <summary>
    /// Gets the shared <see cref="Meter"/> instance.
    /// </summary>
    Meter Meter { get; }

    /// <summary>
    /// Creates or returns a cached <see cref="Counter{T}"/> instrument.
    /// </summary>
    /// <param name="name">The instrument name.</param>
    /// <param name="unit">An optional measurement unit.</param>
    /// <param name="description">An optional description.</param>
    /// <returns>The counter instrument.</returns>
    Counter<long> CreateCounter(string name, string? unit = null, string? description = null);

    /// <summary>
    /// Creates or returns a cached <see cref="Histogram{T}"/> instrument.</summary>
    /// <param name="name">The instrument name.</param>
    /// <param name="unit">An optional measurement unit.</param>
    /// <param name="description">An optional description.</param>
    /// <returns>The histogram instrument.</returns>
    Histogram<double> CreateHistogram(string name, string? unit = null, string? description = null);
}
