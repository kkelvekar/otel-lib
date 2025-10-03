using System;
using System.Diagnostics.Metrics;
using TelemetryBridge;

namespace Todo.DownstreamApi.Services;

public sealed class TodoMetrics
{
    private readonly Counter<long> _createdCounter;
    private readonly Histogram<double> _readDurationHistogram;
    private readonly Histogram<double> _itemCountHistogram;

    public TodoMetrics(IMetricsHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        _createdCounter = handle.CreateCounter("todo_items_created", description: "Number of todo items created");
        _readDurationHistogram = handle.CreateHistogram("todo_list_read_duration_ms", unit: "ms", description: "Todo list retrieval duration");
        _itemCountHistogram = handle.CreateHistogram("todo_list_size", description: "Number of todo items returned");
    }

    public void RecordCreated() => _createdCounter.Add(1);

    public void RecordRead(TimeSpan duration, int count)
    {
        _readDurationHistogram.Record(duration.TotalMilliseconds);
        _itemCountHistogram.Record(count);
    }
}
