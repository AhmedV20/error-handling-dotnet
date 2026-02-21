# MinimalApiSample

A zero-configuration demonstration of ErrorLens.ErrorHandling with ASP.NET Core Minimal APIs.

## Overview

This sample shows how ErrorLens.ErrorHandling works out-of-the-box with minimal setup. Just add `AddErrorHandling()` and `UseErrorHandling()` — no configuration required.

## Prerequisites

- .NET 8.0 SDK or later

## Running the Sample

```bash
# From the project root
dotnet run --project samples/MinimalApiSample

# Or navigate to the sample directory first
cd samples/MinimalApiSample
dotnet run
```

The API will be available at `http://localhost:5000` (or the port shown in output).

## Endpoints

| Endpoint | Description | Expected Response |
|----------|-------------|-------------------|
| `GET /` | Root endpoint | Returns "ErrorLens.ErrorHandling - Minimal API Sample" |
| `GET /error/generic` | Generic exception | 500 with `code: "INTERNAL_ERROR"` |
| `GET /error/invalid-operation` | Invalid operation | 400 with `code: "INVALID_OPERATION"` |
| `GET /error/argument?name=test` | Valid argument | Returns "Hello, test!" |
| `GET /error/argument` | Missing argument | 400 with `code: "ARGUMENT_NULL"` |
| `GET /error/not-found?id=123` | Key not found | 404 with `code: "KEY_NOT_FOUND"` |
| `GET /error/unauthorized` | Unauthorized access | 401 with `code: "UNAUTHORIZED_ACCESS"` |
| `GET /error/format` | Format exception | 400 with `code: "FORMAT"` |
| `GET /error/timeout` | Timeout exception | 408 with `code: "TIMEOUT"` |
| `GET /error/not-implemented` | Not implemented | 501 with `code: "NOT_IMPLEMENTED"` |

## Example Requests

### Generic Exception

```bash
curl http://localhost:5000/error/generic
```

**Response:**
```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred"
}
```

### Invalid Operation

```bash
curl http://localhost:5000/error/invalid-operation
```

**Response:**
```json
{
  "code": "INVALID_OPERATION",
  "message": "This operation is not allowed"
}
```

### Not Found

```bash
curl http://localhost:5000/error/not-found?id=456
```

**Response:**
```json
{
  "code": "KEY_NOT_FOUND",
  "message": "Item with ID 456 not found"
}
```

### Successful Request

```bash
curl "http://localhost:5000/error/argument?name=Alice"
```

**Response:**
```
Hello, Alice!
```

## Key Features Demonstrated

- **Zero-configuration setup**: Only 2 lines of code needed
- **Automatic error code generation**: Exception types converted to ALL_CAPS codes
- **Proper HTTP status mapping**: Common exception types map to appropriate status codes
- **Clean JSON responses**: Structured error format with `code` and `message`

## Code

The entire setup in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add error handling - that's all you need!
builder.Services.AddErrorHandling();

var app = builder.Build();

// Use error handling middleware
app.UseErrorHandling();

// Demo endpoints that throw various exceptions
app.MapGet("/", () => "ErrorLens.ErrorHandling - Minimal API Sample");

// ... error endpoints ...

app.Run();
```

## Learn More

- [Full Documentation](https://ahmedv20.github.io/error-handling-dotnet/current/)
