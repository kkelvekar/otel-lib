using TelemetryBridge;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTelemetryBridge("Todo.MainApi");
builder.Services.AddControllers();

builder.Services.AddHttpClient("DownstreamApi", (serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["DownstreamApiUrl"]!);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/health", () => "OK");
app.MapControllers();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();


