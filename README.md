# ErrorLens.ErrorHandling

[![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.svg)](https://www.nuget.org/packages/ErrorLens.ErrorHandling)
[![Build](https://github.com/AhmedV20/errorLens-errorhandling/actions/workflows/ci.yml/badge.svg)](https://github.com/AhmedV20/errorLens-errorhandling/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET 6.0+](https://img.shields.io/badge/.NET-10.0%20|%209.0%20|%208.0%20|%207.0%20|%206.0-512BD4)

Automatic, consistent error handling for ASP.NET Core APIs. Transform unhandled exceptions into structured JSON responses with zero configuration — or customize everything via attributes, configuration files, and custom handlers.

## Features

- **Zero-Config Exception Handling** — Unhandled exceptions return structured JSON error responses out of the box
- **Validation Error Details** — Field-level errors with property names, messages, and rejected values
- **Custom JSON Field Names** — Rename any response field (`code` → `type`, `message` → `detail`, etc.)
- **YAML & JSON Configuration** — Configure error codes, messages, HTTP statuses via `appsettings.json` or `errorhandling.yml`
- **Custom Exception Attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]`
- **Custom Exception Handlers** — Register `IApiExceptionHandler` implementations with priority ordering
- **Response Customization** — Add global properties (traceId, timestamp) via `IApiErrorResponseCustomizer`
- **RFC 9457 Problem Details** — Opt-in `application/problem+json` compliant responses
- **Configurable Logging** — Control log levels and stack trace verbosity per HTTP status code
- **Multi-Target Support** — .NET 6.0, 7.0, 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package ErrorLens.ErrorHandling
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
builder.Services.AddExceptionHandler<InfrastructureExceptionHandler>();
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
| `OperationCanceledException` | 400 Bad Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` | 404 Not Found |
| `FileNotFoundException` | 404 Not Found |
| `DirectoryNotFoundException` | 404 Not Found |
| `TimeoutException` | 408 Request Timeout |
| `NotImplementedException` | 501 Not Implemented |
| All others | 500 Internal Server Error |

## Samples

| Sample | Description |
|--------|-------------|
| [`MinimalApiSample`](samples/MinimalApiSample) | Zero-config minimal API setup |
| [`FullApiSample`](samples/FullApiSample) | Controllers, custom handlers, response customizers |
| [`ShowcaseSample`](samples/ShowcaseSample) | All features: YAML config, custom field names, attributes, custom handlers, Problem Details |

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
- [Changelog](CHANGELOG.md)

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License — see [LICENSE](LICENSE) for details.
