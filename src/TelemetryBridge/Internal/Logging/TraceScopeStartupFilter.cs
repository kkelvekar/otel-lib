using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace TelemetryBridge.Internal.Logging;

/// <summary>
/// Automatically adds the <see cref="TraceScopeMiddleware"/> to the ASP.NET Core pipeline.
/// </summary>
internal sealed class TraceScopeStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            ArgumentNullException.ThrowIfNull(app);
            app.UseMiddleware<TraceScopeMiddleware>();
            next(app);
        };
    }
}
