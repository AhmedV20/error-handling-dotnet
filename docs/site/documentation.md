# Documentation

Complete reference for ErrorLens.ErrorHandling — structured error responses for ASP.NET Core REST APIs.

## Installation

::: code-group
```bash [dotnet CLI]
dotnet add package ErrorLens.ErrorHandling
```
```xml [PackageReference]
<PackageReference Include="ErrorLens.ErrorHandling" Version="1.4.0" />
```
:::

::: tip
This library is intended for [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core) projects. It will not work outside of an ASP.NET Core application.
:::

### Optional Integration Packages

```bash
# OpenAPI schema generation (.NET 9+)
dotnet add package ErrorLens.ErrorHandling.OpenApi

# Swashbuckle/Swagger schema generation (.NET 6-8)
dotnet add package ErrorLens.ErrorHandling.Swashbuckle

# FluentValidation integration
dotnet add package ErrorLens.ErrorHandling.FluentValidation
```

| Package | Target Frameworks | Version |
|---------|-------------------|---------|
| `ErrorLens.ErrorHandling` | .NET 6, 7, 8, 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling)](https://www.nuget.org/packages/ErrorLens.ErrorHandling) |
| `ErrorLens.ErrorHandling.OpenApi` | .NET 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi) |
| `ErrorLens.ErrorHandling.Swashbuckle` | .NET 6, 7, 8 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle) |
| `ErrorLens.ErrorHandling.FluentValidation` | .NET 6, 7, 8, 9, 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.FluentValidation)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.FluentValidation) |

## Quick Start

### Minimal API (.NET 6+)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddErrorHandling();

var app = builder.Build();
app.UseErrorHandling();
app.MapGet("/", () => { throw new Exception("Test"); });
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
    options.ExceptionLogging = ExceptionLogging.WithStacktrace;
});

// Option 2: Bind from appsettings.json / YAML
builder.Services.AddErrorHandling(builder.Configuration);
```

### Your First Error Response

Throw any exception from a controller or endpoint:

```csharp
[HttpGet("test")]
public IActionResult Test()
{
    throw new InvalidOperationException("Something went wrong");
}
```

Response (HTTP 400):

```json
{
  "code": "INVALID_OPERATION",
  "message": "Something went wrong"
}
```

> **Note (.NET 8+):** ErrorLens automatically registers `IExceptionHandler` on .NET 8+, so exceptions are handled natively by the ASP.NET Core exception handler pipeline. On .NET 6/7, `UseErrorHandling()` registers middleware instead. Both paths produce identical results.

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

Each stage is independently configurable and replaceable. If any handler or customizer throws, the pipeline returns a safe 500 response to prevent cascading failures.

### Framework Support

| Framework | Integration |
|-----------|-------------|
| .NET 6.0 | `IMiddleware` based |
| .NET 7.0 | `IMiddleware` based |
| .NET 8.0+ | Native `IExceptionHandler` + `IMiddleware` fallback |
| .NET 9.0 | Native `IExceptionHandler` + `IMiddleware` fallback |
| .NET 10.0 | Native `IExceptionHandler` + `IMiddleware` fallback |

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

Override any mapping via [configuration](#configuration) or [`[ResponseStatus]`](#exception-attributes) attributes.

## Error Code Strategies

By default, the exception class name is converted to `ALL_CAPS` format:

| Strategy | Exception Class | Generated Code |
|----------|----------------|----------------|
| `AllCaps` (default) | `UserNotFoundException` | `USER_NOT_FOUND` |
| `AllCaps` | `ArgumentNullException` | `ARGUMENT_NULL` |
| `AllCaps` | `Exception` (base) | `INTERNAL_ERROR` |
| `FullQualifiedName` | `UserNotFoundException` | `MyApp.Exceptions.UserNotFoundException` |
| `KebabCase` | `UserNotFoundException` | `user-not-found` |
| `PascalCase` | `UserNotFoundException` | `UserNotFound` |
| `DotSeparated` | `UserNotFoundException` | `user.not.found` |

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;
});
```

## Configuration

ErrorLens supports both JSON (`appsettings.json`) and YAML (`errorhandling.yml`) configuration using the `ErrorHandling` section name.

### JSON Configuration

```json
{
  "ErrorHandling": {
    "Enabled": true,
    "HttpStatusInJsonResponse": true,
    "DefaultErrorCodeStrategy": "AllCaps",
    "AddPathToError": true,
    "IncludeRejectedValues": true,
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
    },

    "RateLimiting": {
      "ErrorCode": "RATE_LIMIT_EXCEEDED",
      "DefaultMessage": "Too many requests. Please try again later.",
      "IncludeRetryAfterInBody": true,
      "UseModernHeaderFormat": false
    },

    "OpenApi": {
      "DefaultStatusCodes": [400, 404, 409, 422, 500]
    }
  }
}
```

### YAML Configuration

Add YAML support with a single line:

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");  // optional: true, reloadOnChange: false
builder.Services.AddErrorHandling(builder.Configuration);
```

The `AddYamlErrorHandling()` method accepts optional parameters: `optional` (default: `true` — won't throw if file is missing) and `reloadOnChange` (default: `false` — set to `true` to auto-reload on file changes).

```yaml
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  AddPathToError: true
  IncludeRejectedValues: true
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

  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: Too many requests. Please try again later.
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false

  OpenApi:
    DefaultStatusCodes:
      - 400
      - 404
      - 409
      - 422
      - 500
```

### All Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable error handling globally |
| `HttpStatusInJsonResponse` | `bool` | `false` | Include HTTP status code in JSON body |
| `DefaultErrorCodeStrategy` | `enum` | `AllCaps` | `AllCaps`, `FullQualifiedName`, `KebabCase`, `PascalCase`, or `DotSeparated` |
| `SearchSuperClassHierarchy` | `bool` | `false` | Search base classes for config matches |
| `AddPathToError` | `bool` | `true` | Include property path in field errors |
| `IncludeRejectedValues` | `bool` | `true` | Include rejected values in validation errors. Set to `false` to prevent sensitive input (e.g., passwords) from being echoed in responses. |
| `FallbackMessage` | `string` | `"An unexpected error occurred"` | Custom message for unhandled 5xx errors. 4xx exceptions are unaffected. |
| `BuiltInMessages` | `Dictionary<string, string>` | `{}` | Override default messages for built-in handlers (`MESSAGE_NOT_READABLE`, `TYPE_MISMATCH`, `BAD_REQUEST`, `VALIDATION_FAILED`) |
| `OverrideModelStateValidation` | `bool` | `false` | Intercept `[ApiController]` validation |
| `UseProblemDetailFormat` | `bool` | `false` | Enable RFC 9457 Problem Details format |
| `ProblemDetailTypePrefix` | `string` | `https://example.com/errors/` | Type URI prefix for Problem Details |
| `ProblemDetailConvertToKebabCase` | `bool` | `true` | Convert error codes to kebab-case in type URI |
| `ExceptionLogging` | `enum` | `MessageOnly` | `None`, `MessageOnly`, `WithStacktrace` |
| `HttpStatuses` | `Dictionary<string, HttpStatusCode>` | `{}` | Exception type → HTTP status code mappings |
| `Codes` | `Dictionary<string, string>` | `{}` | Exception type or field-specific → error code mappings |
| `Messages` | `Dictionary<string, string>` | `{}` | Exception type or field-specific → message mappings |
| `LogLevels` | `Dictionary<string, LogLevel>` | `{}` | HTTP status code/range → log level (e.g., `"5xx": "Error"`) |
| `FullStacktraceHttpStatuses` | `HashSet<string>` | `{}` | HTTP statuses that force full stack trace logging |
| `FullStacktraceClasses` | `HashSet<string>` | `{}` | Exception types that force full stack trace logging |
| `JsonFieldNames` | `JsonFieldNamesOptions` | *(see [JSON Field Names](#custom-json-field-names))* | Custom JSON field names (11 configurable fields) |
| `RateLimiting` | `RateLimitingOptions` | *(see [Rate Limiting](#rate-limiting))* | Rate limiting response options |
| `OpenApi` | `OpenApiOptions` | `DefaultStatusCodes: {400, 404, 500}` | OpenAPI/Swagger schema generation options |

### HTTP Status in JSON Response

```json
{
  "ErrorHandling": {
    "HttpStatusInJsonResponse": true
  }
}
```

Result:

```json
{
  "status": 404,
  "code": "USER_NOT_FOUND",
  "message": "Could not find user with id 123"
}
```

### Super Class Hierarchy Search

Search base classes when matching configuration:

```json
{
  "ErrorHandling": {
    "SearchSuperClassHierarchy": true,
    "HttpStatuses": {
      "System.InvalidOperationException": 400
    }
  }
}
```

Any exception that extends `InvalidOperationException` will match these settings.

### Configuration Priority

Settings are resolved in this order (highest priority first):

1. **Custom exception handlers** — `IApiExceptionHandler` implementations run first in the pipeline
2. **Inline options** — `Action<ErrorHandlingOptions>` in `AddErrorHandling()`
3. **Configuration binding** — `appsettings.json` or `errorhandling.yml`
4. **Exception attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`
5. **Default conventions** — class name to `ALL_CAPS`, built-in HTTP status mappings

## Custom JSON Field Names

Rename any JSON property in error responses to match your API conventions.

::: code-group
```yaml [YAML]
ErrorHandling:
  JsonFieldNames:
    Code: type
    Message: detail
    Status: statusCode
    FieldErrors: fields
    GlobalErrors: errors
    ParameterErrors: params
    Property: field
    RejectedValue: value
    Path: jsonPath
    Parameter: param
```
```json [JSON]
{
  "ErrorHandling": {
    "JsonFieldNames": {
      "Code": "type",
      "Message": "detail",
      "Status": "statusCode",
      "FieldErrors": "fields"
    }
  }
}
```
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.JsonFieldNames.Code = "type";
    options.JsonFieldNames.Message = "detail";
});
```
:::

### Available Field Names

**Top-Level Response:**

| Option | Default | Description |
|--------|---------|-------------|
| `Code` | `code` | Error code field |
| `Message` | `message` | Error message field |
| `Status` | `status` | HTTP status code field |
| `FieldErrors` | `fieldErrors` | Field errors array |
| `GlobalErrors` | `globalErrors` | Global errors array |
| `ParameterErrors` | `parameterErrors` | Parameter errors array |

**Nested Error Objects:**

| Option | Default | Used In | Description |
|--------|---------|---------|-------------|
| `Property` | `property` | Field errors | Property name |
| `RejectedValue` | `rejectedValue` | Field/parameter errors | Rejected value |
| `Path` | `path` | Field errors | Property path |
| `Parameter` | `parameter` | Parameter errors | Parameter name |
| `RetryAfter` | `retryAfter` | Rate limit responses | Retry-after seconds |

::: tip
`Code` and `Message` are shared — they apply to both the top-level response and nested error objects.
:::

### Before & After

Default:
```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "REQUIRED_NOT_NULL",
      "property": "email",
      "message": "Email is required",
      "path": "email"
    }
  ]
}
```

With custom names:
```json
{
  "type": "VALIDATION_FAILED",
  "detail": "Validation failed",
  "statusCode": 400,
  "fields": [
    {
      "type": "REQUIRED_NOT_NULL",
      "field": "email",
      "detail": "Email is required",
      "jsonPath": "email"
    }
  ]
}
```

All 10 field names are validated at startup — null, empty, and duplicate values are rejected with clear error messages.

## Exception Attributes

Decorate exception classes with attributes to control error responses without configuration files.

### ResponseErrorCode

Sets a custom error code for an exception type:

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId)
        : base($"User {userId} not found") { }
}
```

### ResponseStatus

Sets the HTTP status code for an exception type. Accepts both `HttpStatusCode` enum and `int` values (must be 100-599):

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
[ResponseStatus(HttpStatusCode.NotFound)]
public class UserNotFoundException : Exception { ... }

// Or using int:
[ResponseStatus(404)]
public class UserNotFoundException : Exception { ... }
```

### ResponseErrorProperty

Exposes exception properties as additional fields in the error response:

```csharp
[ResponseErrorCode("INSUFFICIENT_FUNDS")]
[ResponseStatus(HttpStatusCode.UnprocessableEntity)]
public class InsufficientFundsException : Exception
{
    [ResponseErrorProperty("required")]
    public decimal RequiredAmount { get; }

    [ResponseErrorProperty("available")]
    public decimal AvailableAmount { get; }

    public InsufficientFundsException(decimal required, decimal available)
        : base("Insufficient funds")
    {
        RequiredAmount = required;
        AvailableAmount = available;
    }
}
```

Response:

```json
{
  "code": "INSUFFICIENT_FUNDS",
  "message": "Insufficient funds",
  "required": 500.00,
  "available": 123.45
}
```

By default, null properties are omitted. Set `IncludeIfNull = true` to always include them:

```csharp
[ResponseErrorProperty("details", IncludeIfNull = true)]
public string? Details { get; }
```

### Attribute Summary

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[ResponseErrorCode("CODE")]` | Class | Sets a custom error code |
| `[ResponseStatus(HttpStatusCode.NotFound)]` | Class | Sets the HTTP status code (accepts `HttpStatusCode` enum) |
| `[ResponseStatus(404)]` | Class | Sets the HTTP status code (accepts `int`, must be 100-599) |
| `[ResponseErrorProperty("name")]` | Property | Includes the property in the JSON response |

## Custom Exception Handlers

Implement `IApiExceptionHandler` to provide specialized handling for specific exception types.

### Interface

```csharp
public interface IApiExceptionHandler
{
    int Order { get; }
    bool CanHandle(Exception exception);
    ApiErrorResponse Handle(Exception exception);
}
```

### Implementation

```csharp
public class InfrastructureExceptionHandler : IApiExceptionHandler
{
    public int Order => 100;

    public bool CanHandle(Exception exception)
    {
        return exception is DatabaseTimeoutException
            or ServiceUnavailableException;
    }

    public ApiErrorResponse Handle(Exception exception)
    {
        return exception switch
        {
            DatabaseTimeoutException dbEx => new ApiErrorResponse(
                HttpStatusCode.ServiceUnavailable,
                "DATABASE_TIMEOUT",
                dbEx.Message),

            ServiceUnavailableException svcEx => new ApiErrorResponse(
                HttpStatusCode.ServiceUnavailable,
                "SERVICE_UNAVAILABLE",
                svcEx.Message),

            _ => new ApiErrorResponse("INFRASTRUCTURE_ERROR", exception.Message)
        };
    }
}
```

### Registration

```csharp
builder.Services.AddApiExceptionHandler<InfrastructureExceptionHandler>();
```

### Using AbstractApiExceptionHandler

For convenience, extend the base class (default `Order` is `1000`, includes a `CreateResponse()` helper):

```csharp
public class PaymentExceptionHandler : AbstractApiExceptionHandler
{
    public override int Order => 50;

    public override bool CanHandle(Exception exception)
        => exception is PaymentException;

    public override ApiErrorResponse Handle(Exception exception)
    {
        var payEx = (PaymentException)exception;
        var response = CreateResponse(
            HttpStatusCode.PaymentRequired, "PAYMENT_FAILED", payEx.Message);
        response.AddProperty("transactionId", payEx.TransactionId);
        return response;
    }
}
```

### Handler Ordering

Handlers execute in order of their `Order` property (lowest first). The first handler whose `CanHandle()` returns `true` processes the exception.

| Order | Handler | Purpose |
|-------|---------|---------|
| 50 | `AggregateExceptionHandler` | `AggregateException` unwrapping |
| 90 | `ModelStateValidationExceptionHandler` | `[ApiController]` model validation |
| 100 | `ValidationExceptionHandler` | DataAnnotations validation |
| 120 | `JsonExceptionHandler` | JSON parsing errors |
| 130 | `TypeMismatchExceptionHandler` | Type conversion errors |
| 150 | `BadRequestExceptionHandler` | Bad HTTP requests |
| ∞ | `DefaultFallbackHandler` | Catch-all fallback |

### AggregateException Handling

The `AggregateExceptionHandler` (Order 50) automatically handles `AggregateException` — common in async code (`Task.WhenAll`, `Parallel.ForEach`):

- **Single inner exception:** Flattens the aggregate, unwraps the inner exception, and re-dispatches it to the handler pipeline. The response matches what the inner exception would produce on its own.
- **Multiple inner exceptions:** Delegates to the `DefaultFallbackHandler`, which generates the error code from the `AggregateException` class name (`AGGREGATE`).

### Custom Fallback Handler

Replace the built-in `DefaultFallbackHandler` by implementing `IFallbackApiExceptionHandler`. This handler runs when **no** `IApiExceptionHandler` matches:

```csharp
public class SafeFallbackHandler : IFallbackApiExceptionHandler
{
    private readonly ILogger<SafeFallbackHandler> _logger;

    public SafeFallbackHandler(ILogger<SafeFallbackHandler> logger)
        => _logger = logger;

    public ApiErrorResponse Handle(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Type}", exception.GetType().Name);

        var response = new ApiErrorResponse(
            HttpStatusCode.InternalServerError,
            "INTERNAL_SERVER_ERROR",
            "An unexpected error occurred. Please contact support if this persists.");

        response.AddProperty("supportReference", $"ERR-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        return response;
    }
}

// Register — replaces the built-in DefaultFallbackHandler
builder.Services.AddSingleton<IFallbackApiExceptionHandler, SafeFallbackHandler>();
```

::: tip
Use the fallback handler for global cross-cutting concerns like incident tracking or custom safe messages. For exception-type-specific handling, use `IApiExceptionHandler` instead.
:::

## Validation Errors

ErrorLens provides structured validation error responses with field-level detail.

### Enabling Validation Override

By default, `[ApiController]` returns ASP.NET Core's built-in `ProblemDetails` for validation failures. Enable `OverrideModelStateValidation` to use ErrorLens's structured format instead:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

### Validation Model

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }
}
```

### Validation Response

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "INVALID_SIZE",
      "property": "name",
      "message": "Name must be between 2 and 100 characters",
      "rejectedValue": "A",
      "path": "name"
    },
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

### Customizing Validation Codes

Validation error codes and messages can be customized via configuration:

```yaml
ErrorHandling:
  Codes:
    email.Required: EMAIL_IS_REQUIRED
    email.EmailAddress: EMAIL_FORMAT_INVALID
  Messages:
    email.Required: A valid email address is required
```

## Response Customization

Add custom properties to all error responses using `IApiErrorResponseCustomizer`. Customizers run after the exception handler and apply to every response.

### Creating a Customizer

```csharp
public class RequestMetadataCustomizer : IApiErrorResponseCustomizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestMetadataCustomizer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Customize(ApiErrorResponse response)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        response.AddProperty("traceId", context.TraceIdentifier);
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
        response.AddProperty("path", context.Request.Path.Value);
    }
}
```

### Registration

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();
```

Multiple customizers execute in registration order:

```csharp
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();
builder.Services.AddErrorResponseCustomizer<TimestampCustomizer>();
builder.Services.AddErrorResponseCustomizer<UserContextCustomizer>();
```

### Use Cases

**Distributed Tracing:**

```csharp
public class TracingCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            response.AddProperty("traceId", activity.TraceId.ToString());
            response.AddProperty("spanId", activity.SpanId.ToString());
        }
    }
}
```

**Server Information:**

```csharp
public class ServerInfoCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("instance", Environment.MachineName);
        response.AddProperty("environment",
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown");
    }
}
```

### Response Example

```json
{
  "code": "USER_NOT_FOUND",
  "message": "User not found",
  "traceId": "0HNL2K9J4K2L9",
  "timestamp": "2026-02-17T10:30:00.0000000Z",
  "path": "/api/users/123",
  "instance": "WEB-SERVER-01"
}
```

## Replaceable Mappers

Three core interfaces control how exceptions are mapped to responses. All are registered via `TryAddSingleton`, so you can replace any of them with your own implementation:

```csharp
// Replace the error code mapper (controls how exception → error code)
builder.Services.AddSingleton<IErrorCodeMapper, MyErrorCodeMapper>();

// Replace the error message mapper (controls how exception → message)
builder.Services.AddSingleton<IErrorMessageMapper, MyErrorMessageMapper>();

// Replace the HTTP status mapper (controls how exception → HTTP status code)
builder.Services.AddSingleton<IHttpStatusMapper, MyHttpStatusMapper>();
```

This gives you full control over the entire error resolution pipeline without writing custom handlers.

## RFC 9457 Problem Details

ErrorLens supports [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) Problem Details response format as an opt-in feature.

### Enabling Problem Details

::: code-group
```yaml [YAML]
ErrorHandling:
  UseProblemDetailFormat: true
  ProblemDetailTypePrefix: https://api.example.com/errors/
  ProblemDetailConvertToKebabCase: true
```
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.UseProblemDetailFormat = true;
    options.ProblemDetailTypePrefix = "https://api.example.com/errors/";
});
```
:::

### Response Format

When enabled, responses use `application/problem+json` content type:

```json
{
  "type": "https://api.example.com/errors/user-not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "User abc-123 not found",
  "instance": "/api/users/abc-123",
  "code": "USER_NOT_FOUND"
}
```

| Field | RFC 9457 | Description |
|-------|----------|-------------|
| `type` | Required | URI reference identifying the problem type |
| `title` | Required | Short human-readable summary (from HTTP status) |
| `status` | Required | HTTP status code |
| `detail` | Optional | Human-readable explanation (exception message) |
| `instance` | Optional | URI reference for the specific occurrence |
| `code` | Extension | Original error code from ErrorLens |

### Type URI Generation

| Error Code | Type URI |
|-----------|----------|
| `USER_NOT_FOUND` | `https://api.example.com/errors/user-not-found` |
| `VALIDATION_FAILED` | `https://api.example.com/errors/validation-failed` |
| `INTERNAL_ERROR` | `https://api.example.com/errors/internal-error` |

Set `ProblemDetailConvertToKebabCase: false` to use error codes as-is in the type URI.

### Validation Errors with Problem Details

```json
{
  "type": "https://api.example.com/errors/validation-failed",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "fieldErrors": [
    {
      "code": "REQUIRED_NOT_NULL",
      "property": "email",
      "message": "Email is required",
      "path": "email"
    }
  ]
}
```

### Custom Problem Detail Factory

To fully customize Problem Details generation, replace the built-in `IProblemDetailFactory` via DI:

```csharp
builder.Services.AddSingleton<IProblemDetailFactory, MyCustomProblemDetailFactory>();
```

Problem Details format works with all other features — attributes, customizers, custom handlers, and localization all apply transparently.

| Option | Default | Description |
|--------|---------|-------------|
| `UseProblemDetailFormat` | `false` | Enable Problem Details format |
| `ProblemDetailTypePrefix` | `https://example.com/errors/` | URI prefix for type field |
| `ProblemDetailConvertToKebabCase` | `true` | Convert error codes to kebab-case |

## Security

### 5xx Safe Message Behavior

All 5xx-class errors (500-599) automatically return a generic safe message instead of the raw exception message:

```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred"
}
```

This prevents internal details (database connection strings, file paths, stack traces) from leaking to API consumers. The original exception is still logged with full details on the server side.

The fallback message can be customized:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.FallbackMessage = "Contact support at help@example.com";
});
```

Result:

```json
{
  "code": "INTERNAL_SERVER_ERROR",
  "message": "Contact support at help@example.com"
}
```

::: info
4xx errors preserve their original messages since these are typically user-facing and safe to expose.
:::

### Customizable Built-in Handler Messages

Override default messages for built-in exception handlers without writing replacement handler classes:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "Invalid JSON payload";
    options.BuiltInMessages["TYPE_MISMATCH"] = "Invalid data type";
    options.BuiltInMessages["BAD_REQUEST"] = "Invalid request";
    options.BuiltInMessages["VALIDATION_FAILED"] = "Please fix the errors below";
});
```

Available keys (from `DefaultErrorCodes`):

| Key | Handler | Default Message |
|-----|---------|-----------------|
| `MESSAGE_NOT_READABLE` | `JsonExceptionHandler` | `"The request body could not be parsed as valid JSON"` |
| `TYPE_MISMATCH` | `TypeMismatchExceptionHandler` | `"A type conversion error occurred"` |
| `BAD_REQUEST` | `BadRequestExceptionHandler` | `"Bad request"` |
| `VALIDATION_FAILED` | `ValidationExceptionHandler` | `"Validation failed"` |

### Message Sanitization

The `BadRequestExceptionHandler` automatically sanitizes Kestrel-internal error messages, replacing framework-specific details with a safe `"Bad request"` message.

### Startup Validation

The `JsonFieldNames` configuration is validated at application startup:

- **Null or empty values** are rejected with clear error messages
- **Duplicate field names** are detected and reported
- **All properties must be unique** to prevent JSON serialization conflicts

Additionally, the following settings are validated at startup:

| Setting | Validation Rule |
|---------|-----------------|
| `ProblemDetailTypePrefix` | Must be empty or a valid absolute URI |
| `RateLimiting.ErrorCode` | Must not be null or empty |
| `RateLimiting.DefaultMessage` | Must not be null or empty |
| `JsonFieldNames.RetryAfter` | Must not be null or empty; must not duplicate other field names |

Invalid values produce clear error messages at application startup, preventing runtime issues.

## Logging

ErrorLens provides configurable logging for handled exceptions.

### Exception Logging Verbosity

Control how much exception detail is logged:

```yaml
ErrorHandling:
  ExceptionLogging: WithStacktrace
```

| Value | Description |
|-------|-------------|
| `None` | No exception logging at all |
| `MessageOnly` | Log exception message only (default) |
| `WithStacktrace` | Log full exception including stack trace |

**`MessageOnly` (default):**

```
warn: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
```

**`WithStacktrace`:**

```
fail: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
      System.Exception: User was not found
         at MyApp.Services.UserService.GetById(Int32 id)
         at MyApp.Controllers.UsersController.Get(Int32 id)
```

### Log Levels Per HTTP Status

Map HTTP status codes or patterns to log levels:

```yaml
ErrorHandling:
  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug        # More specific overrides range pattern
```

Supported log levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.

### Full Stack Trace Control

Force full stack trace logging for specific HTTP statuses or exception types:

```yaml
ErrorHandling:
  FullStacktraceHttpStatuses:
    - 5xx
    - 400
  FullStacktraceClasses:
    - System.NullReferenceException
    - MyApp.Exceptions.CriticalException
```

### Logging Filters

Implement `ILoggingFilter` to suppress logging for specific exceptions:

```csharp
public class IgnoreNotFoundFilter : ILoggingFilter
{
    public bool ShouldLog(ApiErrorResponse response, Exception exception)
    {
        return response.HttpStatusCode != HttpStatusCode.NotFound;
    }
}

builder.Services.AddSingleton<ILoggingFilter, IgnoreNotFoundFilter>();
```

You can register multiple filters — all filters must return `true` for the exception to be logged.

## OpenTelemetry Tracing

ErrorLens automatically creates `Activity` spans via `System.Diagnostics.Activity` when exceptions are handled. Zero new NuGet dependencies — uses runtime-provided APIs only.

### Setup

Wire the ErrorLens activity source into your OpenTelemetry configuration:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("ErrorLens.ErrorHandling")
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });
```

### Span Details

Each handled exception creates an activity named `ErrorLens.HandleException` with:

| Tag | Description |
|-----|-------------|
| `error.code` | The ErrorLens error code |
| `error.type` | The exception type name |
| `http.response.status_code` | The HTTP status code returned |

An exception event is added using OTel semantic conventions:

| Event Attribute | Description |
|-----------------|-------------|
| `exception.type` | Fully qualified exception type |
| `exception.message` | Exception message |
| `exception.stacktrace` | Exception stack trace |

The activity status is set to `Error`.

### No Listener, No Overhead

When no `ActivityListener` is registered for the `"ErrorLens.ErrorHandling"` source, `Activity.StartActivity()` returns `null` and all tag-setting and event-recording code is skipped. Tracing adds zero measurable overhead when not configured.

## Localization

ErrorLens supports localizing error messages via the `IErrorMessageLocalizer` abstraction. This is opt-in — by default, messages pass through unchanged.

### Enabling Localization

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingLocalization<SharedErrorMessages>();
```

This bridges to `IStringLocalizer<T>` and uses error codes as resource keys.

### Resource Files

Create `.resx` files with error codes as keys:

**Resources/SharedErrorMessages.resx** (default / English):

| Key | Value |
|-----|-------|
| `USER_NOT_FOUND` | User not found. |
| `VALIDATION_FAILED` | One or more validation errors occurred. |

**Resources/SharedErrorMessages.fr.resx** (French):

| Key | Value |
|-----|-------|
| `USER_NOT_FOUND` | Utilisateur introuvable. |
| `VALIDATION_FAILED` | Une ou plusieurs erreurs de validation se sont produites. |

### Request Localization

Configure ASP.NET Core's `RequestLocalizationMiddleware`:

```csharp
var supportedCultures = new[] { "en", "fr", "de", "es" };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en");
    options.AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();
app.UseRequestLocalization();  // Must come before UseErrorHandling()
app.UseErrorHandling();
```

### What Gets Localized

Localization applies to all parts of the error response:

- Top-level `message`
- `fieldErrors[].message`
- `globalErrors[].message`
- `parameterErrors[].message`

### Field-Specific Localization

For field errors, `LocalizeFieldError` tries a **composite key** first (`fieldName.errorCode`), then falls back to the error code alone. This allows different translations for the same validation error on different fields:

```
# Resource file (.resx) keys:
REQUIRED_NOT_NULL          = "This field is required"
email.REQUIRED_NOT_NULL    = "Email address is required"
name.REQUIRED_NOT_NULL     = "Name is required"
```

If the composite key (`email.REQUIRED_NOT_NULL`) is found in the resource file, that translation is used. Otherwise, the generic key (`REQUIRED_NOT_NULL`) is used as a fallback.

## OpenAPI & Swagger Integration

Automatically add error response schemas to your API documentation.

### .NET 9+ (Microsoft.AspNetCore.OpenApi)

```bash
dotnet add package ErrorLens.ErrorHandling.OpenApi
```

```csharp
builder.Services.AddOpenApi();
builder.Services.AddErrorHandlingOpenApi();

var app = builder.Build();
app.MapOpenApi();
```

This registers `ErrorResponseOperationTransformer`, which implements `IOpenApiOperationTransformer`. It automatically adds error response schemas (400, 404, 500 by default) to all API operations.

### .NET 6-8 (Swashbuckle)

```bash
dotnet add package ErrorLens.ErrorHandling.Swashbuckle
```

```csharp
builder.Services.AddSwaggerGen();
builder.Services.AddErrorHandlingSwashbuckle();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
```

This registers `ErrorResponseOperationFilter`, which implements `IOperationFilter`.

### How It Works

Both packages share the same `ErrorResponseSchemaGenerator` internally:

- Adds error response schemas for each status code in `DefaultStatusCodes`
- Skips status codes already declared via `[ProducesResponseType]`
- Reflects `UseProblemDetailFormat` in generated schemas
- Respects custom `JsonFieldNamesOptions` for property naming

### Custom Status Codes

```csharp
// .NET 9+
builder.Services.AddErrorHandlingOpenApi(options =>
{
    options.DefaultStatusCodes = new HashSet<int> { 400, 401, 404, 409, 422, 500 };
});

// .NET 6-8
builder.Services.AddErrorHandlingSwashbuckle(options =>
{
    options.DefaultStatusCodes = new HashSet<int> { 400, 401, 403, 422, 500 };
});
```

| Option | Default | Description |
|--------|---------|-------------|
| `DefaultStatusCodes` | `{ 400, 404, 500 }` | HTTP status codes to generate error schemas for |

## Rate Limiting

ErrorLens provides structured error responses for ASP.NET Core rate limiting rejections. Available on .NET 7+ only.

### Setup

Wire the `IRateLimitResponseWriter` into ASP.NET Core's rate limiter:

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
    });

    // THIS IS REQUIRED for ErrorLens structured JSON responses
    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, token);
    };
});
```

### Applying Rate Limiting

::: code-group
```csharp [Controllers]
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]  // Apply to entire controller
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new[] { "Product1", "Product2" });

    [HttpPost("import")]
    [EnableRateLimiting("strict")]  // Different policy per action
    public IActionResult Import([FromBody] ImportRequest request) => Ok();
}
```
```csharp [Minimal APIs]
// Apply to specific endpoint
app.MapGet("/products", () => Results.Ok(new[] { "Product1", "Product2" }))
    .RequireRateLimiting("api");

// Apply to group
var limitedApi = app.MapGroup("/api/orders")
    .RequireRateLimiting("api");

limitedApi.MapGet("/", () => Results.Ok(new[] { "Order1" }));
limitedApi.MapPost("/", (Order order) => Results.Created($"/api/orders/{order.Id}", order));
```
:::

### Response Format

When rate limit is exceeded:

**Headers:**
```
HTTP/1.1 429 Too Many Requests
Retry-After: 42
Content-Type: application/json
```

**Body:**
```json
{
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 42
}
```

### Configuration

::: code-group
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.RateLimiting.ErrorCode = "RATE_LIMIT_EXCEEDED";
    options.RateLimiting.DefaultMessage = "Slow down! Try again shortly.";
    options.RateLimiting.IncludeRetryAfterInBody = true;
    options.RateLimiting.UseModernHeaderFormat = true;
});
```
```yaml [YAML]
ErrorHandling:
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```
:::

| Option | Default | Description |
|--------|---------|-------------|
| `ErrorCode` | `RATE_LIMIT_EXCEEDED` | Error code in the response |
| `DefaultMessage` | `Too many requests. Please try again later.` | Default error message |
| `IncludeRetryAfterInBody` | `true` | Include `retryAfter` property in JSON body |
| `UseModernHeaderFormat` | `false` | Use IETF draft `RateLimit` header format |

### Middleware Order

```csharp
app.UseRequestLocalization();  // 1. Culture (before error messages)
app.UseErrorHandling();        // 2. Exception handling
app.UseRateLimiter();          // 3. Rate limiting (after error handling)
app.MapControllers();          // 4. Endpoints
```

Rate limit messages are localized through the same `IErrorMessageLocalizer` pipeline. See [Localization](#localization) for setup details.

## Built-in Error Code Constants

The `DefaultErrorCodes` static class provides all built-in error code strings for consistent matching in frontend applications.

**General Errors:**

| Constant | Value |
|----------|-------|
| `InternalServerError` | `INTERNAL_SERVER_ERROR` |
| `ValidationFailed` | `VALIDATION_FAILED` |
| `MessageNotReadable` | `MESSAGE_NOT_READABLE` |
| `TypeMismatch` | `TYPE_MISMATCH` |
| `AccessDenied` | `ACCESS_DENIED` |
| `Unauthorized` | `UNAUTHORIZED` |
| `NotFound` | `NOT_FOUND` |
| `MethodNotAllowed` | `METHOD_NOT_ALLOWED` |
| `BadRequest` | `BAD_REQUEST` |
| `ClientClosed` | `CLIENT_CLOSED` |

**Validation-Specific Codes:**

| Constant | Value |
|----------|-------|
| `RequiredNotNull` | `REQUIRED_NOT_NULL` |
| `RequiredNotBlank` | `REQUIRED_NOT_BLANK` |
| `RequiredNotEmpty` | `REQUIRED_NOT_EMPTY` |
| `InvalidSize` | `INVALID_SIZE` |
| `InvalidEmail` | `INVALID_EMAIL` |
| `InvalidPattern` | `REGEX_PATTERN_VALIDATION_FAILED` |
| `ValueOutOfRange` | `VALUE_OUT_OF_RANGE` |
| `InvalidUrl` | `INVALID_URL` |
| `InvalidCreditCard` | `INVALID_CREDIT_CARD` |
| `InvalidLength` | `INVALID_LENGTH` |
| `InvalidMin` | `VALUE_TOO_LOW` |
| `InvalidMax` | `VALUE_TOO_HIGH` |

**Rate Limiting:**

| Constant | Value |
|----------|-------|
| `RateLimitExceeded` | `RATE_LIMIT_EXCEEDED` |

Use these constants in your code: `DefaultErrorCodes.ValidationFailed`, `DefaultErrorCodes.NotFound`, etc.

## API Reference

### Extension Methods

```csharp
// Zero-config setup
services.AddErrorHandling();

// With inline options
services.AddErrorHandling(options => { ... });

// With IConfiguration binding
services.AddErrorHandling(configuration);

// Register custom exception handler
services.AddApiExceptionHandler<THandler>();

// Register response customizer
services.AddErrorResponseCustomizer<TCustomizer>();

// Enable error message localization
services.AddErrorHandlingLocalization<TResource>();

// YAML configuration
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Configuration.AddYamlErrorHandling("custom-path.yml", optional: true, reloadOnChange: true);

// Middleware
app.UseErrorHandling();

// OpenAPI (.NET 9+)
services.AddErrorHandlingOpenApi();
services.AddErrorHandlingOpenApi(options => { ... });

// Swashbuckle (.NET 6-8)
services.AddErrorHandlingSwashbuckle();
services.AddErrorHandlingSwashbuckle(options => { ... });
```

### Models

**ApiErrorResponse:**

```csharp
public class ApiErrorResponse
{
    public string Code { get; set; }
    public string? Message { get; set; }
    public int Status { get; set; }
    public List<ApiFieldError>? FieldErrors { get; set; }
    public List<ApiGlobalError>? GlobalErrors { get; set; }
    public List<ApiParameterError>? ParameterErrors { get; set; }
    public Dictionary<string, object?>? Properties { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }

    public ApiErrorResponse(string code);
    public ApiErrorResponse(string code, string? message);
    public ApiErrorResponse(HttpStatusCode statusCode, string code, string? message);

    public void AddProperty(string name, object? value);
    public void AddFieldError(ApiFieldError fieldError);
    public void AddGlobalError(ApiGlobalError globalError);
    public void AddParameterError(ApiParameterError parameterError);
}
```

**ApiFieldError:**

```csharp
public class ApiFieldError
{
    public string Code { get; set; }
    public string Property { get; set; }
    public string Message { get; set; }
    public object? RejectedValue { get; set; }
    public string? Path { get; set; }
}
```

**ApiGlobalError:**

```csharp
public class ApiGlobalError
{
    public string Code { get; set; }
    public string Message { get; set; }
}
```

**ApiParameterError:**

```csharp
public class ApiParameterError
{
    public string Code { get; set; }
    public string Parameter { get; set; }
    public string Message { get; set; }
    public object? RejectedValue { get; set; }
}
```

### Interfaces

**IApiExceptionHandler:**

```csharp
public interface IApiExceptionHandler
{
    int Order { get; }
    bool CanHandle(Exception exception);
    ApiErrorResponse Handle(Exception exception);
}
```

**IFallbackApiExceptionHandler:**

```csharp
public interface IFallbackApiExceptionHandler
{
    ApiErrorResponse Handle(Exception exception);
}
```

**IApiErrorResponseCustomizer:**

```csharp
public interface IApiErrorResponseCustomizer
{
    void Customize(ApiErrorResponse response);
}
```

**ILoggingFilter:**

```csharp
public interface ILoggingFilter
{
    bool ShouldLog(ApiErrorResponse response, Exception exception);
}
```

**ILoggingService:**

```csharp
public interface ILoggingService
{
    void LogException(Exception exception, ApiErrorResponse response);
}
```

Default implementation: `LoggingService` — logs exceptions using `ILogger` with configurable log levels per HTTP status range. Respects `ILoggingFilter` instances and `ExceptionLogging` option.

**IErrorCodeMapper:**

```csharp
public interface IErrorCodeMapper
{
    string GetErrorCode(Exception exception);
    string GetErrorCode(string fieldSpecificKey, string defaultCode);
}
```

**IErrorMessageMapper:**

```csharp
public interface IErrorMessageMapper
{
    string? GetErrorMessage(Exception exception);
    string GetErrorMessage(string fieldSpecificKey, string defaultCode, string defaultMessage);
}
```

**IHttpStatusMapper:**

```csharp
public interface IHttpStatusMapper
{
    HttpStatusCode GetHttpStatus(Exception exception);
    HttpStatusCode GetHttpStatus(Exception exception, HttpStatusCode defaultStatus);
}
```

**IProblemDetailFactory:**

```csharp
public interface IProblemDetailFactory
{
    ProblemDetailResponse CreateFromApiError(ApiErrorResponse apiError);
}
```

**IErrorMessageLocalizer:**

```csharp
public interface IErrorMessageLocalizer
{
    string? Localize(string errorCode, string? defaultMessage);
    string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage);
}
```

**IRateLimitResponseWriter (.NET 7+):**

```csharp
public interface IRateLimitResponseWriter
{
    Task WriteRateLimitResponseAsync(
        HttpContext context,
        RateLimitLease lease,
        CancellationToken cancellationToken = default);
}
```

### Telemetry

```csharp
public static class ErrorHandlingActivitySource
{
    public const string ActivitySourceName = "ErrorLens.ErrorHandling";
    public static ActivitySource Source { get; }
}
```

### Attributes

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ResponseErrorCodeAttribute : Attribute
{
    public string Code { get; }
    public ResponseErrorCodeAttribute(string code);
}

[AttributeUsage(AttributeTargets.Class)]
public class ResponseStatusAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public ResponseStatusAttribute(HttpStatusCode statusCode);
    public ResponseStatusAttribute(int statusCode);
}

[AttributeUsage(AttributeTargets.Property)]
public class ResponseErrorPropertyAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IncludeIfNull { get; set; }
    public ResponseErrorPropertyAttribute();
    public ResponseErrorPropertyAttribute(string name);
}
```

## Configuration Template

Full YAML configuration template with all available options and their defaults.

```yaml
# ErrorLens.ErrorHandling Configuration Template
# Copy this file to your project as 'errorhandling.yml'
# and configure as needed.

ErrorHandling:
  # Enable or disable error handling globally
  Enabled: true

  # Include HTTP status code in JSON response body
  HttpStatusInJsonResponse: false

  # Error code generation strategy: AllCaps or FullQualifiedName
  DefaultErrorCodeStrategy: AllCaps

  # Search exception base class hierarchy for configuration matches
  SearchSuperClassHierarchy: false

  # Include property path in field error responses
  AddPathToError: true

  # Include rejected values in validation field errors (set false for sensitive data)
  IncludeRejectedValues: true

  # Intercept [ApiController] model validation to use ErrorLens format
  OverrideModelStateValidation: false

  # Use RFC 9457 Problem Details format
  UseProblemDetailFormat: false

  # URI prefix for Problem Details 'type' field
  ProblemDetailTypePrefix: "https://example.com/errors/"

  # Convert error codes to kebab-case in Problem Details 'type' field
  ProblemDetailConvertToKebabCase: true

  # Exception logging verbosity: None, MessageOnly, WithStacktrace
  ExceptionLogging: MessageOnly

  # Custom JSON field names for error responses
  JsonFieldNames:
    Code: code
    Message: message
    Status: status
    FieldErrors: fieldErrors
    GlobalErrors: globalErrors
    ParameterErrors: parameterErrors
    Property: property
    RejectedValue: rejectedValue
    Path: path
    Parameter: parameter

  # Exception type -> HTTP status code mappings
  HttpStatuses:
    # MyApp.Exceptions.UserNotFoundException: 404
    # MyApp.Exceptions.DuplicateEmailException: 409
    # MyApp.Exceptions.ForbiddenException: 403

  # Exception type or field.validation -> error code mappings
  Codes:
    # MyApp.Exceptions.UserNotFoundException: USER_NOT_FOUND
    # email.Required: EMAIL_IS_REQUIRED
    # email.EmailAddress: EMAIL_FORMAT_INVALID

  # Exception type or field.validation -> error message mappings
  Messages:
    # MyApp.Exceptions.UserNotFoundException: The requested user was not found
    # email.Required: A valid email address is required

  # HTTP status code or pattern -> log level mappings
  LogLevels:
    # 4xx: Warning
    # 5xx: Error
    # 404: Debug

  # HTTP status codes/patterns that force full stack trace logging
  FullStacktraceHttpStatuses:
    # - 5xx
    # - 400

  # Exception types that force full stack trace logging
  FullStacktraceClasses:
    # - System.NullReferenceException
    # - MyApp.Exceptions.CriticalException

  # OpenAPI schema generation settings
  OpenApi:
    DefaultStatusCodes:
      - 400
      - 404
      - 500

  # Rate limiting response settings (.NET 7+)
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```

## Troubleshooting

Common issues and solutions when using ErrorLens.ErrorHandling.

### Rate limiting returns plain text, not structured JSON

You must wire up the `IRateLimitResponseWriter` in the `OnRejected` callback. See the [Rate Limiting setup](#setup-1) section. Without this callback, rate limit rejections return ASP.NET Core's default plain text response.

### Validation errors not using ErrorLens format

Validation errors return the default ASP.NET Core `ProblemDetails` format instead of ErrorLens's `fieldErrors` format. Enable `OverrideModelStateValidation`:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

### Custom exception handler not being called

1. Ensure you registered it: `builder.Services.AddApiExceptionHandler<MyCustomHandler>();`
2. Check the `Order` property — lower values execute first
3. Verify `CanHandle()` returns `true` for your exception type
4. Check if a [built-in handler](#handler-ordering) is handling it first (built-in handlers have order 50-150)

### YAML configuration not loading

1. Ensure you called `builder.Configuration.AddYamlErrorHandling("errorhandling.yml");`
2. Verify the file path is correct relative to the application root
3. Ensure the file is copied to output directory in your `.csproj`:

```xml
<ItemGroup>
  <Content Include="errorhandling.yml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

4. Bind configuration: `builder.Services.AddErrorHandling(builder.Configuration);`

### appsettings.json configuration not working

Ensure the section name matches exactly (case-sensitive):

```json
{
  "ErrorHandling": {
    "Enabled": true
  }
}
```

Then bind with: `builder.Services.AddErrorHandling(builder.Configuration);`

### Localized messages not showing

1. Ensure you called `builder.Services.AddErrorHandlingLocalization<YourResource>();`
2. Verify resource files exist with error codes as keys
3. Add `app.UseRequestLocalization()` middleware **before** `app.UseErrorHandling()`
4. Ensure the client sends the `Accept-Language` header

### Error schemas not showing in Swagger/OpenAPI

**For .NET 9+:** Ensure you called `builder.Services.AddErrorHandlingOpenApi();`

**For .NET 6-8:** Ensure you called `builder.Services.AddErrorHandlingSwashbuckle();`

### Middleware order issues

Ensure correct middleware order:

```csharp
app.UseRequestLocalization();  // First (if using localization)
app.UseErrorHandling();        // Catch all errors
app.UseRateLimiter();          // After error handling
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();          // Endpoints last
```

### Package compatibility

| Package | .NET Version |
|---------|--------------|
| `ErrorLens.ErrorHandling` | 6.0, 7.0, 8.0, 9.0, 10.0 |
| `ErrorLens.ErrorHandling.OpenApi` | 9.0+ only |
| `ErrorLens.ErrorHandling.Swashbuckle` | 6.0, 7.0, 8.0 |

Don't install `ErrorLens.ErrorHandling.OpenApi` on .NET 8 or earlier.

### Getting Help

1. **Check the samples** — [MinimalApiSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/MinimalApiSample), [FullApiSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/FullApiSample), [ShowcaseSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/ShowcaseSample), [IntegrationSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/IntegrationSample)
2. **Check existing issues** — [GitHub Issues](https://github.com/AhmedV20/error-handling-dotnet/issues)
3. **Create a new issue** with: .NET version, ErrorLens version, code sample, expected vs actual behavior
