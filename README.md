[![Banner](https://raw.githubusercontent.com/AhmedV20/error-handling-dotnet/main/banner.png)](https://github.com/AhmedV20/error-handling-dotnet)

# ErrorLens.ErrorHandling

[![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.svg)](https://www.nuget.org/packages/ErrorLens.ErrorHandling)
[![NuGet OpenApi](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi.svg?label=OpenApi)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi)
[![NuGet Swashbuckle](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle.svg?label=Swashbuckle)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle)
[![Build](https://github.com/AhmedV20/error-handling-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/AhmedV20/error-handling-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET 6.0+](https://img.shields.io/badge/.NET-10.0%20|%209.0%20|%208.0%20|%207.0%20|%206.0-512BD4)

Transform unhandled exceptions into clean, structured JSON responses with zero configuration required — just two lines of code and you're ready for production. As your needs evolve, take full control with declarative attributes on your exception classes, comprehensive YAML/JSON configuration files for global settings, custom error handlers for complex transformation logic, validation error mapping, multi-language localization support, OpenTelemetry tracing integration, and RFC 9457 Problem Details compliance — all while maintaining automatic sensitive data sanitization and comprehensive logging.

## Features

- **Zero-Config Exception Handling** — Unhandled exceptions return structured JSON error responses out of the box
- **Validation Error Details** — Field-level errors with property names, messages, and rejected values
- **Secure Error Responses** — 5xx errors return generic safe messages to prevent information disclosure
- **Custom JSON Field Names** — Rename any response field (`code` → `type`, `message` → `detail`, etc.)
- **YAML & JSON Configuration** — Configure error codes, messages, HTTP statuses via `appsettings.json` or `errorhandling.yml`
- **Custom Exception Attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]`
- **Custom Exception Handlers** — Register `IApiExceptionHandler` implementations with priority ordering (aggregate unwrapping built-in)
- **Response Customization** — Add global properties (traceId, timestamp) via `IApiErrorResponseCustomizer`
- **RFC 9457 Problem Details** — Opt-in `application/problem+json` compliant responses
- **Configurable Logging** — Control log levels and stack trace verbosity per HTTP status code
- **Startup Validation** — JSON field names validated at startup (non-null, non-empty, unique)
- **Multi-Target Support** — .NET 6.0, 7.0, 8.0, 9.0, and 10.0
- **OpenTelemetry Tracing** — Automatic `Activity` spans with error tags and OTel semantic conventions (zero dependencies)
- **Error Message Localization** — `IErrorMessageLocalizer` with `IStringLocalizer` bridge for multi-language error messages
- **OpenAPI Schema Generation** — Auto-add error response schemas to .NET 9+ OpenAPI docs (`ErrorLens.ErrorHandling.OpenApi`)
- **Swashbuckle Integration** — Auto-add error response schemas to Swagger docs for .NET 6-8 (`ErrorLens.ErrorHandling.Swashbuckle`)
- **Rate Limiting** — Structured 429 responses with `Retry-After` headers via `IRateLimitResponseWriter` (.NET 7+)


## 📦 Packages

| Package | Description | Target Frameworks | Version |
|---------|-------------|-------------------|---------|
| **[ErrorLens.ErrorHandling](https://www.nuget.org/packages/ErrorLens.ErrorHandling)** | Core middleware for structured error responses | .NET 6, 7, 8, 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling) |
| **[ErrorLens.ErrorHandling.OpenApi](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi)** | OpenAPI schema generation (.NET 9+) | .NET 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi) |
| **[ErrorLens.ErrorHandling.Swashbuckle](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle)** | Swashbuckle integration (.NET 6-8) | .NET 6, 7, 8 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle?style=flat-square&color=5b6ee1&label=)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle) |


## Installation

```bash
dotnet add package ErrorLens.ErrorHandling
```

### Optional Integration Packages

```bash
# OpenAPI schema generation for .NET 9+ (Microsoft.AspNetCore.OpenApi)
dotnet add package ErrorLens.ErrorHandling.OpenApi

# Swagger schema generation for .NET 6-8 (Swashbuckle)
dotnet add package ErrorLens.ErrorHandling.Swashbuckle
```

## Quick Start

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddErrorHandling();

var app = builder.Build();
app.UseErrorHandling();
app.MapControllers();
app.Run();
```

All unhandled exceptions now return structured JSON:

```json
{
  "code": "INVALID_OPERATION",
  "message": "The operation is not valid."
}
```

## Configuration

### JSON (`appsettings.json`)

```json
{
  "ErrorHandling": {
    "Enabled": true,
    "HttpStatusInJsonResponse": true,
    "DefaultErrorCodeStrategy": "AllCaps",
    "AddPathToError": true,
    "OverrideModelStateValidation": true,
    "SearchSuperClassHierarchy": true,
    "ExceptionLogging": "WithStacktrace",

    "JsonFieldNames": {
      "Code": "type",
      "Message": "detail"
    },

    "HttpStatuses": {
      "MyApp.UserNotFoundException": 404,
      "MyApp.DuplicateEmailException": 409
    },

    "Codes": {
      "MyApp.UserNotFoundException": "USER_NOT_FOUND",
      "email.Required": "EMAIL_REQUIRED"
    },

    "Messages": {
      "MyApp.UserNotFoundException": "The requested user was not found"
    },

    "LogLevels": {
      "4xx": "Warning",
      "5xx": "Error",
      "404": "Debug"
    }
  }
}
```

### YAML (`errorhandling.yml`)

```yaml
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  AddPathToError: true
  OverrideModelStateValidation: true
  SearchSuperClassHierarchy: true
  ExceptionLogging: WithStacktrace

  JsonFieldNames:
    Code: type
    Message: detail

  HttpStatuses:
    MyApp.UserNotFoundException: 404
    MyApp.DuplicateEmailException: 409

  Codes:
    MyApp.UserNotFoundException: USER_NOT_FOUND
    email.Required: EMAIL_REQUIRED

  Messages:
    MyApp.UserNotFoundException: The requested user was not found

  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug
```

To use YAML configuration:

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Services.AddErrorHandling(builder.Configuration);
```

A full YAML template with all options is available at [`docs/errorhandling-template.yml`](docs/errorhandling-template.yml).

## Custom JSON Field Names

Rename any JSON property in error responses to match your API conventions:

```yaml
ErrorHandling:
  JsonFieldNames:
    Code: type            # "code" → "type"
    Message: detail       # "message" → "detail"
    Status: statusCode    # "status" → "statusCode"
    FieldErrors: fields   # "fieldErrors" → "fields"
    Property: field       # "property" → "field"
```

Before:
```json
{ "code": "USER_NOT_FOUND", "message": "User not found" }
```

After:
```json
{ "type": "USER_NOT_FOUND", "detail": "User not found" }
```

## Custom Exception Attributes

Decorate exception classes with attributes to control error responses:

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

## Custom Exception Handlers

Register custom handlers for specialized exception types:

```csharp
public class InfrastructureExceptionHandler : IApiExceptionHandler
{
    public int Order => 50; // Lower = higher priority (runs before built-in handlers at 100+)

    public bool CanHandle(Exception ex) => ex is DatabaseTimeoutException;

    public ApiErrorResponse Handle(Exception ex) =>
        new(HttpStatusCode.ServiceUnavailable, "DATABASE_TIMEOUT", ex.Message);
}

// Register
builder.Services.AddApiExceptionHandler<InfrastructureExceptionHandler>();
```

## Response Customization

Add global properties to all error responses:

```csharp
public class TraceIdCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("traceId", Activity.Current?.Id);
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
    }
}

// Register
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();
```

## Validation Errors

Validation exceptions automatically include field-level details. Enable `OverrideModelStateValidation: true` to intercept `[ApiController]` automatic validation and use ErrorLens structured format:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "REQUIRED_NOT_NULL",
      "property": "email",
      "message": "Email is required",
      "rejectedValue": null,
      "path": "email"
    }
  ]
}
```

## RFC 9457 Problem Details

Enable RFC 9457 compliant `application/problem+json` responses:

```json
{
  "ErrorHandling": {
    "UseProblemDetailFormat": true,
    "ProblemDetailTypePrefix": "https://api.example.com/errors/",
    "ProblemDetailConvertToKebabCase": true
  }
}
```

Response:

```json
{
  "type": "https://api.example.com/errors/user-not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "User abc-123 not found",
  "code": "USER_NOT_FOUND"
}
```

## Security

The library includes several security-focused features to prevent information disclosure and protect your API:

### 5xx Safe Message Behavior

All 5xx-class errors (500-599) automatically return a generic safe message instead of the raw exception message:

```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred"
}
```

This prevents internal details (database connection strings, file paths, stack traces) from leaking to API consumers. The original exception is still logged with full details on the server side.

**Note**: 4xx errors (400-499) preserve their original messages since these are typically user-facing and safe to expose.

### Message Sanitization

The `BadRequestExceptionHandler` automatically sanitizes Kestrel-internal error messages, replacing framework-specific details with user-safe equivalents:

```json
{
  "code": "BAD_REQUEST",
  "message": "Bad request"
}
```

This prevents internal framework implementation details from being exposed.

### Startup Validation

The `JsonFieldNames` configuration is validated at application startup:

- **Null or empty values** are rejected with clear error messages
- **Duplicate field names** are detected and reported
- **All properties must be unique** to prevent JSON serialization conflicts

This fails-fast behavior prevents misconfiguration from causing runtime errors.

## Logging

Control logging verbosity per HTTP status code or exception type:

```yaml
ErrorHandling:
  ExceptionLogging: WithStacktrace   # None | MessageOnly | WithStacktrace
  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug
  FullStacktraceHttpStatuses:
    - 5xx
  FullStacktraceClasses:
    - MyApp.CriticalException
```

## Error Code Strategies

| Strategy | Example Input | Output |
|----------|--------------|--------|
| `AllCaps` (default) | `UserNotFoundException` | `USER_NOT_FOUND` |
| `FullQualifiedName` | `UserNotFoundException` | `MyApp.Exceptions.UserNotFoundException` |

## Default HTTP Status Mappings

| Exception Type | HTTP Status |
|---------------|-------------|
| `ArgumentException` / `ArgumentNullException` | 400 Bad Request |
| `InvalidOperationException` | 400 Bad Request |
| `FormatException` | 400 Bad Request |
| `OperationCanceledException` | 499 Client Closed Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` | 404 Not Found |
| `FileNotFoundException` | 404 Not Found |
| `DirectoryNotFoundException` | 404 Not Found |
| `TimeoutException` | 408 Request Timeout |
| `NotImplementedException` | 501 Not Implemented |
| All others | 500 Internal Server Error |

## OpenTelemetry Tracing

ErrorLens automatically creates `Activity` spans when handling exceptions — zero configuration needed. Just wire up your OpenTelemetry collector:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ErrorLens.ErrorHandling")  // Subscribe to ErrorLens activities
        .AddConsoleExporter());
```

Each error-handling span includes tags: `error.code`, `error.type`, `http.response.status_code`, plus an exception event with OTel semantic conventions.

## Error Message Localization

Opt-in localization for error messages using ASP.NET Core's `IStringLocalizer`:

```csharp
builder.Services.AddErrorHandlingLocalization<SharedResource>();
```

Error codes are used as resource keys. When a translation exists, it replaces the default message. Falls back to the original message when no translation is found.

## OpenAPI / Swagger Integration

Automatically add error response schemas to your API documentation:

```csharp
// .NET 9+ (Microsoft.AspNetCore.OpenApi)
builder.Services.AddErrorHandlingOpenApi();

// .NET 6-8 (Swashbuckle)
builder.Services.AddErrorHandlingSwashbuckle();
```

Auto-generates schemas for 400, 404, and 500 responses on all endpoints. Respects `[ProducesResponseType]` attributes and reflects your `JsonFieldNames` and `UseProblemDetailFormat` settings.

## Rate Limiting

Write structured 429 responses from ASP.NET Core's rate limiter:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices.GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(context.HttpContext, context.Lease, token);
    };
});
```

Response includes `Retry-After` header and structured JSON body. Configurable via `ErrorHandling:RateLimiting` section.

## Samples

| Sample | Description |
|--------|-------------|
| [`MinimalApiSample`](samples/MinimalApiSample) | Zero-config minimal API setup |
| [`FullApiSample`](samples/FullApiSample) | Controllers, custom handlers, response customizers |
| [`ShowcaseSample`](samples/ShowcaseSample) | All features: YAML config, custom field names, attributes, custom handlers, Problem Details, Swashbuckle integration |
| [`IntegrationSample`](samples/IntegrationSample) | New v1.3.0 features: OpenTelemetry tracing, localization, OpenAPI schemas, rate limiting |

## Documentation

- [Getting Started](docs/guides/getting-started.md)
- [Configuration](docs/guides/configuration.md)
- [Validation Errors](docs/guides/validation-errors.md)
- [Custom Handlers](docs/features/custom-handlers.md)
- [Attributes](docs/features/attributes.md)
- [Response Customization](docs/features/response-customization.md)
- [Problem Details (RFC 9457)](docs/features/problem-details.md)
- [JSON Field Names](docs/features/json-field-names.md)
- [Logging](docs/guides/logging.md)
- [API Reference](docs/reference/api-reference.md)
- [YAML Template](docs/errorhandling-template.yml)
- [Telemetry](docs/features/telemetry.md)
- [Localization](docs/features/localization.md)
- [OpenAPI Integration](docs/features/openapi.md)
- [Swashbuckle Integration](docs/features/swashbuckle.md)
- [Rate Limiting](docs/features/rate-limiting.md)
- [Changelog](CHANGELOG.md)

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License — see [LICENSE](LICENSE) for details.
