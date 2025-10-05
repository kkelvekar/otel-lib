using TelemetryBridge;
using Todo.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTelemetryBridge("Todo.Worker");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
