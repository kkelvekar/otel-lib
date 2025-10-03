using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Ensures the trace scope handler participates in every <see cref="HttpClient"/> created by the factory.
/// </summary>
internal sealed class TraceScopeHttpClientOptions : IConfigureOptions<HttpClientFactoryOptions>
{
    public void Configure(HttpClientFactoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.HttpMessageHandlerBuilderActions.Add(builder =>
        {
            var handler = builder.Services.GetRequiredService<TraceScopeHandler>();
            builder.AdditionalHandlers.Add(handler);
        });
    }
}
