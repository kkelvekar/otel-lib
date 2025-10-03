# TelemetryBridge Demo

TelemetryBridge Demo showcases a reusable OpenTelemetry bootstrapper for .NET 8 services. The solution contains:

- **TelemetryBridge** – class library that wires tracing, metrics, and developer log mirroring.
- **Todo.MainApi** – public facing API that proxies todo requests to the downstream service.
- **Todo.DownstreamApi** – in-memory todo API that exposes CRUD endpoints and emits custom metrics.
- **Todo.Worker** – background worker exercising telemetry outside of ASP.NET Core.
- **OtelCollector** – Dockerized OpenTelemetry Collector configured for Azure Application Insights.

## Prerequisites

- .NET SDK 8.0+
- Docker Desktop (with Linux containers enabled)

## Building

```bash
dotnet build TelemetryBridgeDemo.sln -warnaserror
```

## Running locally

1. Start the OpenTelemetry Collector (only needs to run once):

   ```bash
   docker compose up --build -d
   ```

   Replace the `APPLICATIONINSIGHTS_CONNECTION_STRING` value in `docker-compose.yml` with a real Application Insights connection string before running in a real environment.

2. Run the downstream API:

   ```bash
   dotnet run --project src/Todo.DownstreamApi
   ```

3. In a new terminal, run the main API:

   ```bash
   dotnet run --project src/Todo.MainApi
   ```

4. In another terminal, run the worker service:

   ```bash
   dotnet run --project src/Todo.Worker
   ```

## Smoke testing

With the downstream API listening on `http://localhost:5071` and the main API on `http://localhost:5072`, try the following commands:

```bash
curl http://localhost:5071/health
curl http://localhost:5072/health
curl http://localhost:5072/todo
curl -X POST http://localhost:5072/todo \
  -H "Content-Type: application/json" \
  -d '{"title":"demo"}'
```

Each API call is automatically traced. Developer logs from the `Todo.*` namespaces are attached to spans as events and shipped to the collector alongside traces and metrics via OTLP gRPC (`http://localhost:4317` by default). Override the OTLP endpoint and protocol using the `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL` environment variables if needed.

The worker continuously emits telemetry every 10 seconds, demonstrating non-HTTP consumers.

## Telemetry pipeline

```
Todo services & worker → OTLP (4317/4318) → OpenTelemetry Collector → Azure Application Insights
```

The collector forwards traces, metrics, and filtered logs (dropping `Microsoft.*` and `System.*` categories) to Application Insights.

## Project layout

```
TelemetryBridgeDemo.sln
├── src/
│   ├── TelemetryBridge/          # reusable library
│   ├── Todo.MainApi/             # main API
│   ├── Todo.DownstreamApi/       # downstream API
│   ├── Todo.Worker/              # background worker
│   └── OtelCollector/            # collector Docker assets
├── tests/
│   └── TelemetryBridge.Tests/    # unit tests for the library
├── docker-compose.yml
└── README.md
```

## Logging and correlation

TelemetryBridge does not add or replace any logger providers. Instead, it decorates `ILogger<T>` to mirror developer logs into active spans as `ActivityEvent` instances, while lightweight middleware and `HttpClient` handlers flow trace identifiers through `ILogger` scopes. Existing logging stacks (Console, Serilog, etc.) continue to function unchanged.
