using TelemetryBridge;
using Todo.DownstreamApi.Repositories;
using Todo.DownstreamApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5071");

builder.Services.AddTelemetryBridge("Todo.DownstreamApi");
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
builder.Services.AddSingleton<TodoMetrics>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;
