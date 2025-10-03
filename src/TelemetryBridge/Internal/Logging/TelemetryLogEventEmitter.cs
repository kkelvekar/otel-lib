using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TelemetryBridge.Internal.Configuration;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Adds log entries as span events on the current activity.
/// </summary>
internal sealed class TelemetryLogEventEmitter : ITelemetryLogEventEmitter
{
    private readonly TelemetryBridgeOptions _options;

    public TelemetryLogEventEmitter(TelemetryBridgeOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void Record<TState>(string category, LogLevel level, EventId eventId, TState state, Exception? exception, string message)
    {
        if (!ShouldCapture(category))
        {
            return;
        }

        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        var tags = new ActivityTagsCollection
        {
            { "log.severity", level.ToString() },
            { "log.category", category },
            { "log.event_id", eventId.Id },
        };

        if (!string.IsNullOrWhiteSpace(eventId.Name))
        {
            tags.Add("log.event_name", eventId.Name!);
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            tags.Add("log.message", message);
        }

        if (exception is not null)
        {
            tags.Add("exception.type", exception.GetType().FullName ?? exception.GetType().Name);
            tags.Add("exception.message", exception.Message);
            tags.Add("exception.stacktrace", exception.StackTrace ?? string.Empty);
        }

        if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
        {
            foreach (var pair in structuredState)
            {
                if (pair.Key is null)
                {
                    continue;
                }

                tags[$"log.state.{pair.Key}"] = pair.Value;
            }
        }

        activity.AddEvent(new ActivityEvent("log", tags));
    }

    private bool ShouldCapture(string category)
        => !string.IsNullOrEmpty(category) && category.StartsWith(_options.DeveloperLogCategoryPrefix, StringComparison.Ordinal);
}
