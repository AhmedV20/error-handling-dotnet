# IntegrationSample

Demonstrates ErrorLens.ErrorHandling integration with modern .NET ecosystem features.

## Features

- **OpenTelemetry Tracing** - Distributed tracing with error handling spans
- **OpenAPI Schema Generation** (.NET 9+) - Auto-generated error schemas
- **Rate Limiting** - Structured 429 responses with retry information
- **AggregateException Handling** - Single inner exception unwrapping
- **Error Message Localization** - Multi-language error messages via `IStringLocalizer`
- **Validation** - Structured `fieldErrors` with `OverrideModelStateValidation`

## Prerequisites

- .NET 9.0 or later

## Running the Sample

```bash
cd samples/IntegrationSample
dotnet run
```

The application will start on `http://localhost:5000`.

## Endpoints

| Endpoint | Method | Feature | Description |
|----------|--------|---------|-------------|
| `/` | GET | Health check | Returns service information |
| `/error` | GET | Basic error | Throws `InvalidOperationException` |
| `/not-found` | GET | 404 error | Throws `KeyNotFoundException` |
| `/aggregate` | GET | AggregateException | Single inner exception (unwrapped) |
| `/aggregate-multi` | GET | AggregateException | Multiple inner exceptions (fallback) |
| `/limited` | GET | Rate limiting | 5 requests per minute |
| `/localized-error` | GET | Localization | Error with `Accept-Language` support |
| `/validate` | POST | Validation | Structured `fieldErrors` with DataAnnotations |

## Example Requests

### Health Check

```bash
curl http://localhost:5000/
```

Response:

```json
{
  "service": "IntegrationSample",
  "version": "1.4.0",
  "features": [
    "OpenTelemetry Tracing",
    "OpenAPI Schema Generation",
    "Rate Limiting",
    "AggregateException Handling",
    "Error Message Localization",
    "Validation (OverrideModelStateValidation)"
  ]
}
```

### Trigger Rate Limiting

```bash
# Run this multiple times quickly (more than 5 times in 1 minute)
curl http://localhost:5000/limited
```

After 5 requests within 1 minute:

```json
{
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 13
}
```

Headers include:

```
Retry-After: 13
```

### Trigger 404 Error

```bash
curl http://localhost:5000/not-found
```

Response:

```json
{
  "code": "KEY_NOT_FOUND",
  "message": "Resource not found",
  "status": 404
}
```

### AggregateException (Single Inner - Unwrapped)

```bash
curl http://localhost:5000/aggregate
```

Response (single inner exception is unwrapped):

```json
{
  "code": "INVALID_OPERATION",
  "message": "Unwrapped inner exception",
  "status": 400
}
```

### AggregateException (Multiple Inners - Fallback)

```bash
curl http://localhost:5000/aggregate-multi
```

Response (multiple inner exceptions use fallback):

```json
{
  "code": "AGGREGATE",
  "message": "An unexpected error occurred",
  "status": 500
}
```

### Localized Error Messages

```bash
# English (default)
curl http://localhost:5000/localized-error
```

Response:

```json
{
  "code": "INVALID_OPERATION",
  "message": "This is a demo error",
  "status": 400
}
```

```bash
# French — send Accept-Language header
curl -H "Accept-Language: fr" http://localhost:5000/localized-error
```

Response (if French translation exists in `Resources/ErrorMessages.fr.resx`):

```json
{
  "code": "INVALID_OPERATION",
  "message": "Ceci est une erreur de démonstration",
  "status": 400
}
```

```bash
# Spanish
curl -H "Accept-Language: es" http://localhost:5000/localized-error
```

Response (if Spanish translation exists in `Resources/ErrorMessages.es.resx`):

```json
{
  "code": "INVALID_OPERATION",
  "message": "Este es un error de demostración",
  "status": 400
}
```

### Validation Error

```bash
curl -X POST http://localhost:5000/validate \
  -H "Content-Type: application/json" \
  -d '{"name": "A", "email": "bad", "priority": 10}'
```

Response:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "status": 400,
  "fieldErrors": [
    {
      "code": "INVALID_LENGTH",
      "property": "name",
      "message": "Name must be between 2 and 100 characters"
    },
    {
      "code": "INVALID_EMAIL",
      "property": "email",
      "message": "Invalid email format"
    },
    {
      "code": "VALUE_OUT_OF_RANGE",
      "property": "priority",
      "message": "Priority must be between 1 and 5"
    }
  ]
}
```

## OpenAPI Documentation

Access the OpenAPI schema at:

```
http://localhost:5000/openapi/v1.json
```

The ErrorLens integration automatically adds error response schemas for all endpoints.

## Configuration Highlights

### ErrorLens with Configuration

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = true;
    options.OverrideModelStateValidation = true;
    options.IncludeRejectedValues = true;
});
```

### OpenTelemetry with ErrorLens Source

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ErrorLens.ErrorHandling")  // ErrorLens activity source
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());
```

This enables distributed tracing for all error handling operations. You'll see spans for:

- Exception handling events
- Error response generation
- Custom handler execution

### OpenAPI Integration (.NET 9+)

```csharp
builder.Services.AddOpenApi();
builder.Services.AddErrorHandlingOpenApi();  // Add ErrorLens schemas
```

```csharp
app.MapOpenApi();
```

### Rate Limiting with ErrorLens Responses

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
    });

    // Wire up ErrorLens response writer
    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, token);
    };
});
```

```csharp
app.UseRateLimiter();

// Apply to endpoint
app.MapGet("/limited", () => Results.Ok(new { message = "You got through!" }))
    .RequireRateLimiting("api");
```

### Error Message Localization

```csharp
// Register localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddErrorHandlingLocalization<ErrorMessages>();

// Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "fr", "es" };
    options.SetDefaultCulture(supportedCultures[0]);
    options.AddSupportedUICultures(supportedCultures);
    options.FallBackToParentUICultures = true;
});
```

Error codes are used as resource keys in `.resx` files. When a translation exists for the error code, it replaces the default message. Create resource files like `ErrorMessages.fr.resx` with entries keyed by error code (e.g., `INVALID_OPERATION`).

## Middleware Pipeline

```csharp
app.UseRequestLocalization(); // Must be before error handling
app.UseErrorHandling();       // Error handling middleware
app.UseRateLimiter();         // After error handling
app.MapOpenApi();             // OpenAPI endpoint
```

## Key Features Demonstrated

| Feature | Implementation |
|---------|----------------|
| Structured error responses | Automatic for all exceptions |
| HTTP status in JSON | `HttpStatusInJsonResponse = true` |
| Rate limiting | `AddFixedWindowLimiter` with `OnRejected` callback |
| OpenAPI error schemas | `AddErrorHandlingOpenApi()` |
| Distributed tracing | OpenTelemetry with ErrorLens source |
| AggregateException unwrapping | Automatic for single inner exceptions |
| Error message localization | `AddErrorHandlingLocalization<T>()` with `.resx` resources |
| Validation (fieldErrors) | `OverrideModelStateValidation = true` with DataAnnotations |

## Further Reading

- [Rate Limiting](https://ahmedv20.github.io/error-handling-dotnet/current/documentation#rate-limiting) - Complete rate limiting guide
- [OpenTelemetry](https://ahmedv20.github.io/error-handling-dotnet/current/documentation#opentelemetry-tracing) - Distributed tracing setup
- [OpenAPI Integration](https://ahmedv20.github.io/error-handling-dotnet/current/documentation#openapi-swagger-integration) - Schema generation
- [Getting Started](https://ahmedv20.github.io/error-handling-dotnet/current/documentation#installation) - Basic setup guide
