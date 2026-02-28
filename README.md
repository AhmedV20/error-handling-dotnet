[![Banner](https://raw.githubusercontent.com/AhmedV20/error-handling-dotnet/main/banner.png)](https://github.com/AhmedV20/error-handling-dotnet)

# ErrorLens.ErrorHandling

[![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.svg)](https://www.nuget.org/packages/ErrorLens.ErrorHandling)
[![NuGet OpenApi](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi.svg?label=OpenApi)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi)
[![NuGet Swashbuckle](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle.svg?label=Swashbuckle)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle)
[![NuGet FluentValidation](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.FluentValidation.svg?label=FluentValidation)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.FluentValidation)
[![Build](https://github.com/AhmedV20/error-handling-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/AhmedV20/error-handling-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET 6.0+](https://img.shields.io/badge/.NET-10.0%20|%209.0%20|%208.0%20|%207.0%20|%206.0-512BD4)

Transform unhandled exceptions into clean, structured JSON responses with minimal setup — just two lines of code to get started.

ErrorLens provides a production-ready error handling pipeline for ASP.NET Core APIs. Every exception is automatically converted into a consistent, structured JSON response with appropriate HTTP status codes, machine-readable error codes, and human-readable messages. The library handles the full lifecycle from exception to response: handler selection, response customization, secure 5xx safe messaging, configurable logging, and optional localization — with zero-config defaults that work out of the box and deep extensibility when you need it.

> **Full documentation:** [https://ahmedv20.github.io/error-handling-dotnet/current/](https://ahmedv20.github.io/error-handling-dotnet/current/)

---

## Features

- **Zero-Config Exception Handling** — Unhandled exceptions return structured JSON error responses out of the box
- **Validation Error Details** — Field-level errors with property names, messages, and rejected values
- **Secure Error Responses** — 5xx errors return generic safe messages to prevent information disclosure
- **Custom JSON Field Names** — Rename any response field (`code` → `type`, `message` → `detail`, etc.)
- **YAML & JSON Configuration** — Configure error codes, messages, HTTP statuses via `appsettings.json` or `errorhandling.yml`
- **Custom Exception Attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]`
- **Custom Exception Handlers** — Register `IApiExceptionHandler` implementations with priority ordering
- **AggregateException Unwrapping** — Automatically flattens and unwraps single-inner `AggregateException`
- **Response Customization** — Add global properties (traceId, timestamp) via `IApiErrorResponseCustomizer`
- **Replaceable Mappers** — Override `IErrorCodeMapper`, `IErrorMessageMapper`, or `IHttpStatusMapper`
- **RFC 9457 Problem Details** — Opt-in `application/problem+json` compliant responses
- **Configurable Logging** — Control log levels and stack trace verbosity per HTTP status code
- **Startup Validation** — JSON field names validated at startup (non-null, non-empty, unique)
- **Multi-Target Support** — .NET 6.0, 7.0, 8.0, 9.0, and 10.0
- **.NET 8+ Native Integration** — Automatically registers `IExceptionHandler` on .NET 8+; falls back to middleware on .NET 6/7
- **OpenTelemetry Tracing** — Automatic `Activity` spans with error tags and OTel semantic conventions
- **Error Message Localization** — `IErrorMessageLocalizer` with `IStringLocalizer` bridge for multi-language support
- **OpenAPI / Swagger Integration** — Auto-add error response schemas to API docs
- **Rate Limiting** — Structured 429 responses with `Retry-After` headers via `IRateLimitResponseWriter` (.NET 7+)
- **Built-in Error Code Constants** — `DefaultErrorCodes` class with 23 predefined codes for consistent frontend matching
- **FluentValidation Integration** — First-party support for FluentValidation with automatic error code mapping and severity filtering
- **Configurable Error Messages** — Customize 5xx fallback message and override built-in handler messages via `FallbackMessage` and `BuiltInMessages`
- **5 Error Code Strategies** — `AllCaps`, `FullQualifiedName`, `KebabCase`, `PascalCase`, `DotSeparated`

## Quick Start

### Installation

```bash
dotnet add package ErrorLens.ErrorHandling
```

### Minimal API (.NET 6+)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddErrorHandling();

var app = builder.Build();
app.UseErrorHandling();

app.MapGet("/", () => { throw new Exception("Something went wrong"); });
app.Run();
```

### Controller-Based API

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddErrorHandling();

var app = builder.Build();
app.UseErrorHandling();
app.MapControllers();
app.Run();
```

### With Configuration Options

```csharp
// Option 1: Inline options
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = true;
    options.OverrideModelStateValidation = true;
    options.IncludeRejectedValues = true;
    options.ExceptionLogging = ExceptionLogging.WithStacktrace;
});

// Option 2: Bind from appsettings.json / YAML
builder.Services.AddErrorHandling(builder.Configuration);
```

> **Note (.NET 8+):** ErrorLens automatically registers `IExceptionHandler` on .NET 8+, so exceptions are handled natively by the ASP.NET Core exception handler pipeline. On .NET 6/7, `UseErrorHandling()` registers middleware instead. Both paths produce identical results.

All unhandled exceptions now return structured JSON:

```json
{
  "code": "INVALID_OPERATION",
  "message": "The operation is not valid."
}
```

The error code is generated automatically from the exception class name using `ALL_CAPS` strategy — `InvalidOperationException` becomes `INVALID_OPERATION`, `UserNotFoundException` becomes `USER_NOT_FOUND`.

## How It Works

ErrorLens processes exceptions through a pipeline with clearly defined stages:

```
Exception thrown
  → Handler Selection (sorted by Order, first CanHandle() match wins)
    → Fallback Handler (if no handler matches)
      → HTTP Status in JSON (if configured)
        → Response Customizers (all IApiErrorResponseCustomizer run in order)
          → Logging (ILoggingService with ILoggingFilter checks)
            → Localization (IErrorMessageLocalizer replaces messages)
              → OpenTelemetry (Activity enriched with error tags)
                → JSON Response (or Problem Details if enabled)
```

Each stage is independently configurable and replaceable. If any handler or customizer throws, the pipeline catches the error, logs both exceptions, and returns a safe 500 response to prevent cascading failures.

For a detailed architecture overview with Mermaid diagrams, see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Packages

| Package | Description | Target Frameworks | Version |
|---------|-------------|-------------------|---------|
| **[ErrorLens.ErrorHandling](https://www.nuget.org/packages/ErrorLens.ErrorHandling)** | Core middleware for structured error responses | .NET 6, 7, 8, 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling) |
| **[ErrorLens.ErrorHandling.OpenApi](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi)** | OpenAPI schema generation (.NET 9+) | .NET 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi) |
| **[ErrorLens.ErrorHandling.Swashbuckle](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle)** | Swashbuckle integration (.NET 6-8) | .NET 6, 7, 8 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle) |
| **[ErrorLens.ErrorHandling.FluentValidation](https://www.nuget.org/packages/ErrorLens.ErrorHandling.FluentValidation)** | FluentValidation integration | .NET 6, 7, 8, 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.FluentValidation?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.FluentValidation) |

```bash
# Optional integration packages
dotnet add package ErrorLens.ErrorHandling.OpenApi        # .NET 9+
dotnet add package ErrorLens.ErrorHandling.Swashbuckle    # .NET 6-8
dotnet add package ErrorLens.ErrorHandling.FluentValidation  # .NET 6-10
```

## Default HTTP Status Mappings

ErrorLens maps common .NET exception types to appropriate HTTP status codes out of the box:

| Exception Type | HTTP Status |
|---------------|-------------|
| `ArgumentException` / `ArgumentNullException` | 400 Bad Request |
| `InvalidOperationException` | 400 Bad Request |
| `FormatException` | 400 Bad Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` | 404 Not Found |
| `FileNotFoundException` / `DirectoryNotFoundException` | 404 Not Found |
| `TimeoutException` | 408 Request Timeout |
| `OperationCanceledException` | 499 Client Closed Request |
| `NotImplementedException` | 501 Not Implemented |
| All others | 500 Internal Server Error |

> **Note:** `TaskCanceledException` inherits from `OperationCanceledException`, so it also maps to 499 automatically.

Override any mapping via [configuration](docs/guides/configuration.md) or [`[ResponseStatus]`](docs/features/attributes.md) attributes.

## Configuration

ErrorLens supports both JSON (`appsettings.json`) and YAML (`errorhandling.yml`) configuration. Here is a minimal example:

```json
{
  "ErrorHandling": {
    "HttpStatusInJsonResponse": true,
    "OverrideModelStateValidation": true,
    "IncludeRejectedValues": true,
    "ExceptionLogging": "WithStacktrace",
    "HttpStatuses": {
      "MyApp.UserNotFoundException": 404
    },
    "Codes": {
      "MyApp.UserNotFoundException": "USER_NOT_FOUND"
    }
  }
}
```

For YAML configuration:

```yaml
ErrorHandling:
  HttpStatusInJsonResponse: true
  OverrideModelStateValidation: true
  IncludeRejectedValues: true
  ExceptionLogging: WithStacktrace
  HttpStatuses:
    MyApp.UserNotFoundException: 404
  Codes:
    MyApp.UserNotFoundException: USER_NOT_FOUND
```

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Services.AddErrorHandling(builder.Configuration);
```

Settings are resolved in this order (highest priority first):

1. **Custom exception handlers** — `IApiExceptionHandler` implementations
2. **Inline options** — `Action<ErrorHandlingOptions>` in `AddErrorHandling()`
3. **Configuration binding** — `appsettings.json` or `errorhandling.yml`
4. **Exception attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`
5. **Default conventions** — class name to `ALL_CAPS`, built-in HTTP status mappings

For the full configuration reference (all options, JSON field names, rate limiting, OpenAPI), see the [Configuration Guide](docs/guides/configuration.md).

## Exception Attributes

Decorate exception classes with attributes to control error responses declaratively:

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
[ResponseStatus(HttpStatusCode.NotFound)]
public class UserNotFoundException : Exception
{
    [ResponseErrorProperty("userId")]
    public string UserId { get; }

    public UserNotFoundException(string userId)
        : base($"User {userId} not found") => UserId = userId;
}
```

Response:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "User abc-123 not found",
  "userId": "abc-123"
}
```

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[ResponseErrorCode("CODE")]` | Class | Sets a custom error code |
| `[ResponseStatus(HttpStatusCode.NotFound)]` | Class | Sets the HTTP status code (accepts `HttpStatusCode` enum) |
| `[ResponseStatus(404)]` | Class | Sets the HTTP status code (accepts `int`, must be 100-599) |
| `[ResponseErrorProperty("name")]` | Property | Includes the property in the JSON response |

For more details, see [Exception Attributes](docs/features/attributes.md).

## Validation Errors

Enable `OverrideModelStateValidation` to get structured field-level validation errors:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

Response:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "INVALID_EMAIL",
      "property": "email",
      "message": "Invalid email format",
      "rejectedValue": "bad",
      "path": "email"
    }
  ]
}
```

Validation error codes and messages can be customized via configuration. See the [Configuration Guide](docs/guides/configuration.md) for details.

## Security

All 5xx-class errors (500-599) automatically return a generic safe message instead of the raw exception message. The default message can be customized via `ErrorHandlingOptions.FallbackMessage`:

```json
{
  "code": "INTERNAL_SERVER_ERROR",
  "message": "An unexpected error occurred"
}
```

This prevents internal details (database connection strings, file paths, stack traces) from leaking to API consumers. The original exception is still logged with full details on the server side. The `BadRequestExceptionHandler` also sanitizes Kestrel-internal error messages automatically.

## Samples

| Sample | Description | Key Features |
|--------|-------------|--------------|
| [`MinimalApiSample`](samples/MinimalApiSample) | Zero-config minimal API | Default error handling, automatic error codes |
| [`FullApiSample`](samples/FullApiSample) | Controller-based API with extensibility | Custom handlers, response customizers, logging filters, Swagger |
| [`ShowcaseSample`](samples/ShowcaseSample) | Full feature showcase with YAML | YAML config, JSON field names, attributes, Problem Details, Swashbuckle |
| [`IntegrationSample`](samples/IntegrationSample) | Modern .NET ecosystem integration | OpenTelemetry, localization, OpenAPI, rate limiting |

## Documentation

> **Documentation site:** [https://ahmedv20.github.io/error-handling-dotnet/current/](https://ahmedv20.github.io/error-handling-dotnet/current/)

**Guides**
- [Getting Started](docs/guides/getting-started.md) — Installation, setup, first error response
- [Configuration](docs/guides/configuration.md) — JSON/YAML config, all options, priority order
- [Logging](docs/guides/logging.md) — Log levels, stack traces, logging filters
- [Troubleshooting](docs/site/guide/troubleshooting.md) — Common issues and solutions

**Core Features**
- [Exception Attributes](docs/features/attributes.md) — `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]`
- [Custom Handlers](docs/features/custom-handlers.md) — `IApiExceptionHandler`, `IFallbackApiExceptionHandler`
- [Response Customization](docs/features/response-customization.md) — `IApiErrorResponseCustomizer`
- [JSON Field Names](docs/features/json-field-names.md) — Rename any response field
- [Problem Details (RFC 9457)](docs/features/problem-details.md) — `application/problem+json` format

**Integration Features**
- [OpenTelemetry Tracing](docs/features/telemetry.md) — Automatic `Activity` spans
- [Localization](docs/features/localization.md) — Multi-language error messages
- [OpenAPI (.NET 9+)](docs/features/openapi.md) — Auto-generated error schemas
- [Swashbuckle (.NET 6-8)](docs/features/swashbuckle.md) — Swagger error schemas
- [Rate Limiting (.NET 7+)](docs/features/rate-limiting.md) — Structured 429 responses
- [FluentValidation](docs/site/documentation.md#fluentvalidation-integration) — FluentValidation integration with automatic error mapping

**Reference**
- [Architecture](docs/ARCHITECTURE.md) — Full architecture guide with Mermaid diagrams
- [API Reference](docs/reference/api-reference.md) — All public types, interfaces, and extension methods
- [Changelog](CHANGELOG.md) — Version history

## Prerequisites

- **.NET SDK** 6.0 or later (multi-targeting builds require .NET 10.0 SDK)
- **ASP.NET Core** application (Minimal API or Controller-based)

## Building from Source

```bash
git clone https://github.com/AhmedV20/error-handling-dotnet.git
cd error-handling-dotnet
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running Sample Projects

```bash
dotnet run --project samples/MinimalApiSample
dotnet run --project samples/ShowcaseSample
```

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License — see [LICENSE](LICENSE) for details.
