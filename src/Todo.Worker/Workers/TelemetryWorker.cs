using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Todo.Worker.Metrics;

namespace Todo.Worker.Workers;

public sealed class TelemetryWorker : BackgroundService
{
    private static readonly Action<ILogger, int, Exception?> WorkerTickRecorded = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(1, nameof(WorkerTickRecorded)),
        "Worker tick {Tick}");

    private static readonly Action<ILogger, int, Exception?> WorkerWarningSample = LoggerMessage.Define<int>(
        LogLevel.Warning,
        new EventId(2, nameof(WorkerWarningSample)),
        "Worker warning sample at tick {Tick}");

    private static readonly Action<ILogger, int, Exception?> WorkerSimulatedError = LoggerMessage.Define<int>(
        LogLevel.Error,
        new EventId(3, nameof(WorkerSimulatedError)),
        "Worker experienced a simulated error on tick {Tick}");

    private static readonly Action<ILogger, Exception?> DownstreamHealthCheckFailed = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(4, nameof(DownstreamHealthCheckFailed)),
        "Failed to reach downstream API");

    private readonly ILogger<TelemetryWorker> _logger;
    private readonly WorkerMetrics _metrics;
    private readonly IHttpClientFactory _httpClientFactory;

    public TelemetryWorker(ILogger<TelemetryWorker> logger, WorkerMetrics metrics, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        var tick = 0;

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            tick++;
            WorkerTickRecorded(_logger, tick, null);
            WorkerWarningSample(_logger, tick, null);
            _metrics.RecordHeartbeat();

            try
            {
                if (tick % 3 == 0)
                {
                    throw new InvalidOperationException("Simulated worker failure");
                }
            }
            catch (InvalidOperationException ex)
            {
                WorkerSimulatedError(_logger, tick, ex);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("downstream");
                using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), stoppingToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                DownstreamHealthCheckFailed(_logger, ex);
            }
        }
    }
}
