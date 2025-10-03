using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Ensures trace context is promoted to the logging scope for inbound requests.
/// </summary>
internal sealed class TraceScopeMiddleware : IMiddleware
{
    private readonly ILogger<TraceScopeMiddleware> _logger;

    public TraceScopeMiddleware(ILogger<TraceScopeMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var activity = Activity.Current;
        if (activity is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        using (_logger.BeginScope(TraceScopeValues.Create(activity)))
        {
            await next(context).ConfigureAwait(false);
        }
    }
}
