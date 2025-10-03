Project Overview

This repository defines a .NET 8 telemetry bridge project.
The goal is to provide a reusable library (TelemetryBridge) that automatically collects traces, metrics, and developer logs from consumer applications and exports them via OpenTelemetry to a local OTel Collector, which in turn sends data to Azure Application Insights.

In addition, there are consumer applications (two simple Todo APIs and a Worker Service) and a Dockerized OTel Collector with a docker-compose.yml for local testing.

Key Principles

Agents must follow these principles when generating or modifying code:

1. Single Extension Entry Point

Consumers integrate by calling only:

builder.Services.AddTelemetryBridge(serviceName: "<APP_NAME>");

No additional configuration from consumers.



2. Automatic Tracing

Consumers should never create Activity spans manually.

Tracing is captured automatically via ASP.NET Core, HttpClient, and gRPC instrumentation.



3. Logs Handling

Do not register or replace ILoggerProvider.

Developer logs are captured and correlated as span events (with severity, category, exception data).

The OTel Collector filters out framework noise (Microsoft.*, System.*) and forwards developer logs only to Application Insights.



4. SOLID, OOP, Best Practices

Library code must be clean, testable, and minimal on its public API surface.

Warnings are treated as errors.

Nullable reference types enabled.



5. Collector Config

OTLP receivers on 4317 (gRPC) and 4318 (HTTP).

Batch processor + filter (exclude framework logs).

Export traces, metrics, and logs to Azure Application Insights using APPLICATIONINSIGHTS_CONNECTION_STRING.



6. Consumer Apps

Todo.MainApi: Calls DownstreamApi and logs at different severities.

Todo.DownstreamApi: Simple in-memory CRUD with developer logs and metrics.

Todo.Worker: Background job producing logs and metrics.

All consumers call only AddTelemetryBridge(...).



7. Environment Config

Library defaults OTLP endpoint to http://localhost:4317.

Can be overridden by env vars:

OTEL_EXPORTER_OTLP_ENDPOINT

OTEL_EXPORTER_OTLP_PROTOCOL


Collector requires:

APPLICATIONINSIGHTS_CONNECTION_STRING




8. Acceptance Criteria

Build with zero warnings.

APIs respond at /health.

/todo roundtrips from Main → Downstream.

Worker logs visible.

Telemetry flows: Consumers → Collector → Azure Application Insights.

Only developer logs (all severities) exported; framework logs excluded.

No manual Activity creation required.




Instructions to Agents

When extending this project:

Always preserve library independence (never add consumer-side configuration).

Ensure OTel Collector YAML keeps logs filtering intact.

Validate build pipelines with dotnet build -warnaserror.

Update README.md if user-facing commands or environment variables change.

Include unit tests for guard clauses and configuration logic.

Document all non-obvious design decisions with inline comments
