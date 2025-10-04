using System;
using System.Diagnostics.Metrics;
using TelemetryBridge;

namespace Todo.Worker.Metrics;

internal sealed class WorkerMetrics
{
    private readonly Counter<long> _heartbeatCounter;

    public WorkerMetrics(IMetricsHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        _heartbeatCounter = handle.CreateCounter("worker_ticks", description: "Number of worker loops executed");
    }

    public void RecordHeartbeat() => _heartbeatCounter.Add(1);
}
