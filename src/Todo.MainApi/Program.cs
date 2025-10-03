using System;
using TelemetryBridge;
using Todo.MainApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5072");

builder.Services.AddTelemetryBridge("Todo.MainApi");
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient<IDownstreamTodoClient, DownstreamTodoClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5071");
});

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;
