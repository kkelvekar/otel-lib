using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Adds trace context scopes for outbound HTTP calls created via <see cref="IHttpClientFactory"/>.
/// </summary>
internal sealed class TraceScopeHandler : DelegatingHandler
{
    private readonly ILogger<TraceScopeHandler> _logger;

    public TraceScopeHandler(ILogger<TraceScopeHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var activity = Activity.Current;
        if (activity is null)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        using (_logger.BeginScope(TraceScopeValues.Create(activity)))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
