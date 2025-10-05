using System.Diagnostics.Metrics;

namespace Todo.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _counter;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _meter = new Meter("Todo.Worker");
        _counter = _meter.CreateCounter<int>("worker-runs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            _logger.LogWarning("This is a warning message.");
            _logger.LogError("This is an error message.");
            _counter.Add(1);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
