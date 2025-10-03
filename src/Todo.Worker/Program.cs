using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelemetryBridge;
using Todo.Worker.Metrics;
using Todo.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTelemetryBridge("Todo.Worker");
builder.Services.AddHttpClient("downstream", client => client.BaseAddress = new Uri("http://localhost:5071"));
builder.Services.AddSingleton<WorkerMetrics>();
builder.Services.AddHostedService<TelemetryWorker>();

var app = builder.Build();
await app.RunAsync();
