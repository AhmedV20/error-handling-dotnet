# Getting Started

## Installation

```bash
dotnet add package ErrorLens.ErrorHandling
```

## Setup

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

### With Configuration Binding

```csharp
builder.Services.AddErrorHandling(builder.Configuration);
```

### With Inline Options

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = true;
    options.ExceptionLogging = ExceptionLogging.WithStacktrace;
});
```

## Your First Error Response

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

## How Error Codes Are Generated

By default, the exception class name is converted to `ALL_CAPS` format:

| Exception Class | Error Code |
|----------------|------------|
| `InvalidOperationException` | `INVALID_OPERATION` |
| `ArgumentNullException` | `ARGUMENT` |
| `UserNotFoundException` | `USER_NOT_FOUND` |
| `Exception` (base) | `INTERNAL_ERROR` |

## Default HTTP Status Mappings

| Exception Type | HTTP Status |
|---------------|-------------|
| `InvalidOperationException` | 400 Bad Request |
| `ArgumentException` / `ArgumentNullException` | 400 Bad Request |
| `FormatException` | 400 Bad Request |
| `OperationCanceledException` | 499 Client Closed Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` / `FileNotFoundException` | 404 Not Found |
| `DirectoryNotFoundException` | 404 Not Found |
| `TimeoutException` | 408 Request Timeout |
| `NotImplementedException` | 501 Not Implemented |
| All others | 500 Internal Server Error |

## Next Steps

- [Configuration](configuration.md) — Customize error codes, messages, and HTTP statuses
- [Validation Errors](validation-errors.md) — Handle DataAnnotations validation
- [Attributes](../features/attributes.md) — Decorate exceptions with attributes
