# ShowcaseSample

A comprehensive demonstration of all ErrorLens.ErrorHandling features using YAML configuration.

## Overview

This sample showcases the full capabilities of ErrorLens.ErrorHandling:
- YAML configuration with `errorhandling.yml`
- Custom JSON field names (`code` → `type`, `message` → `detail`)
- Six feature-specific controllers
- Custom exception handler and response customizer
- Swagger UI for testing

## Prerequisites

- .NET 8.0 SDK or later

## Running the Sample

```bash
# From project root
dotnet run --project samples/ShowcaseSample

# Or navigate to sample directory first
cd samples/ShowcaseSample
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

## Project Structure

```
ShowcaseSample/
├── Controllers/
│   ├── BasicErrorsController.cs      # Zero-config exception handling
│   ├── ValidationController.cs       # DataAnnotations + IValidatableObject
│   ├── AttributesController.cs       # Attribute-based customization
│   ├── ConfigDrivenController.cs     # YAML configuration mappings
│   ├── ProblemDetailsController.cs   # RFC 9457 Problem Details
│   └── FeaturesController.cs         # FullQualifiedName strategy demo
├── Customizers/
│   └── RequestMetadataCustomizer.cs  # Adds traceId, timestamp, path
├── Exceptions/
│   ├── AuthExceptions.cs             # Authorization exceptions
│   ├── DomainExceptions.cs           # Business domain exceptions
│   ├── InfrastructureExceptions.cs   # Infrastructure exceptions (config-driven)
│   └── CustomExceptions.cs           # Exceptions for strategy demo
├── Handlers/
│   └── InfrastructureExceptionHandler.cs # Custom handler (alternative to config)
├── Models/
│   └── RequestModels.cs              # Validation request models
├── errorhandling.yml                 # YAML configuration
└── Program.cs                        # Startup configuration
```

## YAML Configuration

The `errorhandling.yml` file demonstrates all configuration options:

```yaml
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  SearchSuperClassHierarchy: true
  ExceptionLogging: WithStacktrace
  OverrideModelStateValidation: true
  IncludeRejectedValues: true

  # Customize JSON field names
  JsonFieldNames:
    Code: type          # Renames "code" to "type"
    Message: detail     # Renames "message" to "detail"

  # Force full stacktrace logging for server errors
  FullStacktraceHttpStatuses:
    - 5xx

  # Map exception types to HTTP status codes
  HttpStatuses:
    ShowcaseSample.Exceptions.DatabaseTimeoutException: 503
    ShowcaseSample.Exceptions.ServiceUnavailableException: 503
    ShowcaseSample.Exceptions.RateLimitExceededException: 429

  # Map exception types to error codes
  Codes:
    ShowcaseSample.Exceptions.DatabaseTimeoutException: DATABASE_TIMEOUT
    ShowcaseSample.Exceptions.ServiceUnavailableException: SERVICE_UNAVAILABLE
    ShowcaseSample.Exceptions.RateLimitExceededException: RATE_LIMIT_EXCEEDED
    email.Required: EMAIL_IS_REQUIRED
    email.EmailAddress: EMAIL_FORMAT_INVALID

  # Map exception types to custom messages
  Messages:
    ShowcaseSample.Exceptions.DatabaseTimeoutException: A database operation timed out. Please try again.
    ShowcaseSample.Exceptions.ServiceUnavailableException: An external service is temporarily unavailable.
    email.Required: A valid email address is required

  # Configure log levels per HTTP status range
  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug

  # RFC 9457 Problem Details (toggle to switch response format)
  UseProblemDetailFormat: false
  ProblemDetailTypePrefix: https://api.example.com/errors/
  ProblemDetailConvertToKebabCase: true
```

## Feature Matrix

| Controller | Features Demonstrated |
|-------------|------------------------|
| **BasicErrors** | Zero-config handling, default error codes |
| **Validation** | DataAnnotations, `fieldErrors` format, OverrideModelStateValidation |
| **Attributes** | `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]` |
| **ConfigDriven** | YAML-based HTTP status/code/message mappings |
| **ProblemDetails** | RFC 9457 Problem Details format (set `UseProblemDetailFormat: true` in YAML to enable) |
| **Features** | FullQualifiedName error code strategy, additional feature demos |

## API Endpoints

### Basic Errors Controller

Zero-configuration exception handling.

| Endpoint | Description | Response |
|----------|-------------|----------|
| `GET /api/basicerrors/generic` | Generic exception | 500 INTERNAL_ERROR |
| `GET /api/basicerrors/invalid-operation` | Invalid operation | 400 INVALID_OPERATION |
| `GET /api/basicerrors/argument-null` | Argument null | 400 ARGUMENT_NULL |
| `GET /api/basicerrors/key-not-found` | Key not found | 404 KEY_NOT_FOUND |
| `GET /api/basicerrors/timeout` | Timeout | 408 TIMEOUT |

### Validation Controller

DataAnnotations validation with structured `fieldErrors` and `globalErrors`.

| Endpoint | Description | Request |
|----------|-------------|---------|
| `POST /api/validation/users` | Field-level validation | `{ "name": "...", "email": "..." }` |
| `POST /api/validation/transfer` | Range validation | `{ "amount": -5 }` |
| `POST /api/validation/change-password` | Cross-field validation (`globalErrors`) | `{ "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }` |

### Attributes Controller

Attribute-based exception customization.

| Endpoint | Description | Response Includes |
|----------|-------------|-------------------|
| `GET /api/attributes/user/{id}` | User not found | `userId` property |
| `POST /api/attributes/register` | Duplicate email | `email` property |
| `POST /api/attributes/transfer` | Insufficient funds | `required`, `available` properties |
| `GET /api/attributes/forbidden` | Forbidden | `requiredRole` property |

### Config Driven Controller

YAML configuration-driven mappings.

| Endpoint | Exception | Mapped Status | Mapped Code |
|----------|-----------|----------------|--------------|
| `GET /api/configdriven/db-timeout` | DatabaseTimeoutException | 503 | DATABASE_TIMEOUT |
| `GET /api/configdriven/service-down` | ServiceUnavailableException | 503 | SERVICE_UNAVAILABLE |
| `GET /api/configdriven/rate-limit` | RateLimitExceededException | 429 | RATE_LIMIT_EXCEEDED |

### Problem Details Controller

RFC 9457 Problem Details format. **To enable:** set `UseProblemDetailFormat: true` in `errorhandling.yml`.

| Endpoint | Description | Format |
|----------|-------------|--------|
| `GET /api/problemdetails/not-found` | Problem Details response | RFC 9457 |
| `GET /api/problemdetails/server-error` | Server error in PD format | RFC 9457 |
| `GET /api/problemdetails/forbidden` | Forbidden with extensions | RFC 9457 + extensions |

> **Note:** Problem Details is disabled by default. When disabled, these endpoints return the standard ErrorLens format with custom field names (`type`/`detail`). Toggle `UseProblemDetailFormat` in `errorhandling.yml` to switch.

### Features Controller

Error code strategy demos. Works with both `AllCaps` (default) and `FullQualifiedName` strategies.

| Endpoint | Description | AllCaps Code | FullQualifiedName Code |
|----------|-------------|--------------|------------------------|
| `GET /api/features/inventory-error` | Inventory shortage | `INSUFFICIENT_INVENTORY` | `ShowcaseSample.Exceptions.InsufficientInventoryException` |
| `GET /api/features/payment-declined` | Payment failure | `PAYMENT_DECLINED` | `ShowcaseSample.Exceptions.PaymentDeclinedException` |

> To switch strategy, change `DefaultErrorCodeStrategy` in `errorhandling.yml` from `AllCaps` to `FullQualifiedName`.

## Example Requests

### Zero-Config Error

```bash
curl http://localhost:5000/api/basicerrors/key-not-found
```

**Response:**
```json
{
  "type": "KEY_NOT_FOUND",
  "detail": "The requested resource was not found",
  "status": 404,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/basicerrors/key-not-found"
}
```

### Validation Error

```bash
curl -X POST http://localhost:5000/api/validation/users \
  -H "Content-Type: application/json" \
  -d '{"name": "A", "email": "bad"}'
```

**Response:**
```json
{
  "type": "VALIDATION_FAILED",
  "detail": "Validation failed",
  "status": 400,
  "fieldErrors": [
    {
      "property": "email",
      "message": "Invalid email format",
      "code": "EMAIL_FORMAT_INVALID"
    },
    {
      "property": "name",
      "message": "Name must be between 2 and 100 characters",
      "code": "INVALID_LENGTH"
    },
    {
      "property": "password",
      "message": "Password is required",
      "code": "REQUIRED"
    }
  ],
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/validation/users"
}
```

### Attribute-Based Error with Properties

```bash
curl http://localhost:5000/api/attributes/user/12345
```

**Response:**
```json
{
  "type": "USER_NOT_FOUND",
  "detail": "User with ID '12345' was not found",
  "status": 404,
  "userId": "12345",
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/attributes/user/12345"
}
```

### Config-Driven Error

```bash
curl http://localhost:5000/api/configdriven/db-timeout
```

**Response:**
```json
{
  "type": "DATABASE_TIMEOUT",
  "detail": "Database operation 'GetUserById' timed out",
  "status": 503,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/configdriven/db-timeout"
}
```

### Global Errors (Cross-Field Validation)

```bash
curl -X POST http://localhost:5000/api/validation/change-password \
  -H "Content-Type: application/json" \
  -d '{"currentPassword":"pass123","newPassword":"different","confirmPassword":"mismatch"}'
```

**Response:**
```json
{
  "type": "VALIDATION_FAILED",
  "detail": "Validation failed",
  "status": 400,
  "globalErrors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "New password and confirmation password must match."
    }
  ],
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/validation/change-password"
}
```

> `globalErrors` are produced when `IValidatableObject.Validate()` returns `ValidationResult` with `null` member names. This indicates cross-field validation failures that don't apply to a single property.

### Problem Details Format

```bash
curl http://localhost:5000/api/problemdetails/not-found
```

**Response:**
```json
{
  "type": "https://api.example.com/errors/user-not-found",
  "title": "User Not Found",
  "detail": "User with ID 'user-456' was not found",
  "status": 404,
  "instance": "/api/problemdetails/not-found"
}
```

## Custom Components

### InfrastructureExceptionHandler

Custom handler for infrastructure exceptions:

```csharp
public class InfrastructureExceptionHandler : IApiExceptionHandler
{
    public int Order => 100;

    public bool CanHandle(Exception exception)
        => exception is DatabaseTimeoutException
            or ServiceUnavailableException
            or RateLimitExceededException;

    public ApiErrorResponse Handle(Exception exception)
    {
        return exception switch
        {
            DatabaseTimeoutException dbEx => new ApiErrorResponse(
                HttpStatusCode.ServiceUnavailable, "DATABASE_TIMEOUT", dbEx.Message),

            ServiceUnavailableException svcEx => new ApiErrorResponse(
                HttpStatusCode.ServiceUnavailable, "SERVICE_UNAVAILABLE", svcEx.Message),

            RateLimitExceededException rateEx => CreateRateLimitResponse(rateEx),

            _ => new ApiErrorResponse("INFRASTRUCTURE_ERROR", exception.Message)
        };
    }

    private static ApiErrorResponse CreateRateLimitResponse(RateLimitExceededException ex)
    {
        var response = new ApiErrorResponse(
            HttpStatusCode.TooManyRequests, "RATE_LIMIT_EXCEEDED", ex.Message);
        response.AddProperty("retryAfterSeconds", ex.RetryAfterSeconds);
        return response;
    }
}
```

### RequestMetadataCustomizer

Adds request metadata to all error responses:

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

## Custom JSON Field Names

The sample demonstrates custom JSON field names via YAML:

```yaml
JsonFieldNames:
  Code: type       # "code" field becomes "type"
  Message: detail  # "message" field becomes "detail"
```

All error responses use these custom names:
```json
{
  "type": "ERROR_CODE",      // instead of "code"
  "detail": "Error message", // instead of "message"
  "status": 400
}
```

## Learn More

- [Full Documentation](https://ahmedv20.github.io/error-handling-dotnet/current/)
