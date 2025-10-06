using Todo.Worker;
using UBS.AM.Observability;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddObservability("Todo.Worker");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
