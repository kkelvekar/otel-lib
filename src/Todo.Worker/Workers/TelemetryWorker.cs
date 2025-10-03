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
            _logger.LogInformation("Worker tick {Tick}", tick);
            _logger.LogWarning("Worker warning sample at tick {Tick}", tick);
            _metrics.RecordHeartbeat();

            try
            {
                if (tick % 3 == 0)
                {
                    throw new InvalidOperationException("Simulated worker failure");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker experienced a simulated error on tick {Tick}", tick);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("downstream");
                using var response = await client.GetAsync("/health", stoppingToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reach downstream API");
            }
        }
    }
}
