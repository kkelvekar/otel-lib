using TelemetryBridge;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTelemetryBridge("Todo.DownstreamApi");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/health", () => "OK");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
