using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Helper that builds a logging scope dictionary with trace identifiers.
/// </summary>
internal static class TraceScopeValues
{
    public static IReadOnlyDictionary<string, object?> Create(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["TraceId"] = activity.TraceId.ToString(),
            ["SpanId"] = activity.SpanId.ToString(),
        };
    }
}
