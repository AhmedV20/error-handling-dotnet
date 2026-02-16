# Getting Started

## Installation

::: code-group
```bash [dotnet CLI]
dotnet add package ErrorLens.ErrorHandling
```
```xml [PackageReference]
<PackageReference Include="ErrorLens.ErrorHandling" Version="1.3.0" />
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
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` / `FileNotFoundException` | 404 Not Found |
| `DirectoryNotFoundException` | 404 Not Found |
| `TimeoutException` | 408 Request Timeout |
| `OperationCanceledException` | 499 Client Closed Request |
| `NotImplementedException` | 501 Not Implemented |
| All others | 500 Internal Server Error |

## Framework Support

| Framework | Integration |
|-----------|-------------|
| .NET 6.0 | `IMiddleware` based |
| .NET 7.0 | `IMiddleware` based |
| .NET 8.0+ | Native `IExceptionHandler` + `IMiddleware` fallback |
| .NET 9.0 | Native `IExceptionHandler` + `IMiddleware` fallback |
| .NET 10.0 | Native `IExceptionHandler` + `IMiddleware` fallback |

## Validation Exception Handling

::: warning
By default, `[ApiController]` automatic model validation uses ASP.NET Core's built-in `ProblemDetails` response. To use ErrorLens's structured `fieldErrors` format instead, you must enable `OverrideModelStateValidation`:
:::

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

When a model like this fails validation:

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }
}
```

The response includes detailed field-level errors:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "INVALID_SIZE",
      "property": "name",
      "message": "The field Name must be a string with a minimum length of 2 and a maximum length of 100.",
      "path": "name"
    },
    {
      "code": "INVALID_EMAIL",
      "property": "email",
      "message": "Invalid email format",
      "path": "email"
    },
    {
      "code": "VALUE_OUT_OF_RANGE",
      "property": "age",
      "message": "Age must be between 18 and 120",
      "path": "age"
    }
  ]
}
```

## Next Steps

- [Configuration](/guide/configuration) — Customize error codes, messages, and HTTP statuses
- [Exception Attributes](/features/attributes) — Decorate exceptions with attributes
- [Custom Handlers](/features/custom-handlers) — Write your own exception handlers
