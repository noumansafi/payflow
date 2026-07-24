# Architecture: Logging & Observability (ops)

**Status:** Done (ops logging). Audit trail is separate — see [../features/audit-logs.md](../features/audit-logs.md).

## Purpose

Give operators and recruiters a clear story for **how PayFlow diagnoses runtime behaviour**:

- What failed?
- Which request / user?
- How long did it take?

This is **structured operational logging** (Serilog). It is **not** the security/compliance audit table.

| Concern | Mechanism |
|---|---|
| Ops / crash / latency | Serilog → `ILogger<T>` |
| Who moved money / auth events | `AuditLogs` table (`IAuditLogger`) |

## Stack (industry .NET shape)

```text
Application / Middleware code
  → Microsoft.Extensions.Logging.ILogger<T>   (abstraction)
    → Serilog provider                        (implementation)
      → Console sink + rolling File sink
      → Enrichers (LogContext, Machine, Environment)
```

Application **never** references Serilog types. Only `PayFlow.Api` hosts Serilog.

## Request pipeline

```text
HTTP request
  → ExceptionHandlingMiddleware     (ProblemDetails + structured error logs)
  → Serilog request logging         (HTTP method/path/status/elapsed)
  → Authentication / Authorization
  → RequestLogContextMiddleware     (push TraceId, UserId into LogContext)
  → Controllers → MediatR
       → LoggingBehavior            (Handling / Handled {RequestName} + elapsed)
       → ValidationBehavior
       → Handler
```

### Middleware roles

| Component | Role |
|---|---|
| `UseSerilogRequestLogging` | One completion line per HTTP call with message template + status-based level |
| `RequestLogContextMiddleware` | Enriches all in-request `ILogger` calls with `TraceId` / `UserId` |
| `ExceptionHandlingMiddleware` | Maps exceptions to ProblemDetails; logs Error/Warning with method, path, `TraceId` |

### MediatR `LoggingBehavior`

Logs **request type name and duration only** — never serializes command bodies (avoids leaking passwords/tokens).

Example templates:

- `Handling {RequestName}`
- `Handled {RequestName} in {ElapsedMilliseconds} ms`
- `Error handling {RequestName} after {ElapsedMilliseconds} ms`

## Configuration

Serilog is configured under `Serilog` in:

- [`src/PayFlow.Api/appsettings.json`](../../src/PayFlow.Api/appsettings.json)
- [`src/PayFlow.Api/appsettings.Development.json`](../../src/PayFlow.Api/appsettings.Development.json)

Host wiring: bootstrap logger + `builder.Services.AddSerilog(...)` in `Program.cs` (Serilog.AspNetCore current guidance).

**Sinks**

- Console (developer / container stdout)
- Rolling file: `logs/payflow-.log` (7-day retention)

**Enrichment**

- `FromLogContext` — per-request properties
- `WithMachineName`, `WithEnvironmentName`
- Fixed property `Application=PayFlow`
- Request diagnostic context: `TraceId`, `UserId`, `RequestHost`, `ClientIp`

## Correlating a failure

1. Client ProblemDetails includes `traceId`
2. Search console or `logs/payflow-*.log` for that `TraceId`
3. Same property appears on HTTP completion logs, exception logs, and MediatR handling logs for that request

## What we deliberately do **not** log

- Passwords, refresh tokens, JWTs
- Full command/query payloads
- Card/PII dumps (out of scope)

## Why Serilog (and how to replace it)

**Why Serilog:** mature structured logging for .NET, strong message templates, enrichers, and sinks (console/file today; Seq/ELK later) without coupling business code to a vendor.

**How it’s used:** Application and most middleware depend only on `ILogger<T>`. Serilog is a **host provider** in `PayFlow.Api` (`AddSerilog`, request logging, `LogContext` enrichment, `appsettings`).

**Replacing later:** swap the Api composition root — packages, `Program.cs` wiring, Serilog config section, request-logging middleware, and `RequestLogContextMiddleware`. Handlers, `LoggingBehavior`, and Domain/Infrastructure stay on `ILogger<T>` and need little or no change. Audit (`IAuditLogger`) is independent of this choice.

## Evolution path

- Extra sinks / JSON / OpenTelemetry without rewriting Application use cases
- Keep `AuditLogs` for compliance queries (M8 admin API)

## Recruiter takeaway

Ops logging is **abstraction-first** (`ILogger<T>`) with Serilog as a replaceable host plug-in, plus request correlation and safe MediatR templates — separate from the SQL audit trail.
