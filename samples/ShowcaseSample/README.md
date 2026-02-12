# ShowcaseSample

A comprehensive demonstration of all ErrorLens.ErrorHandling features using YAML configuration.

## Overview

This sample showcases the full capabilities of ErrorLens.ErrorHandling:
- YAML configuration with `errorhandling.yml`
- Custom JSON field names (`code` → `type`, `message` → `detail`)
- Five feature-specific controllers
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
│   ├── ValidationController.cs       # DataAnnotations validation
│   ├── AttributesController.cs       # Attribute-based customization
│   ├── ConfigDrivenController.cs     # YAML configuration mappings
│   └── ProblemDetailsController.cs  # RFC 9457 Problem Details
├── Customizers/
│   └── RequestMetadataCustomizer.cs # Adds traceId, timestamp, path
├── Exceptions/
│   ├── AuthExceptions.cs            # Authorization exceptions
│   ├── DomainExceptions.cs          # Business domain exceptions
│   └── InfrastructureExceptions.cs  # Infrastructure exceptions
├── Handlers/
│   └── InfrastructureExceptionHandler.cs # Custom handler for infra exceptions
├── Models/
│   └── RequestModels.cs             # Validation request models
├── errorhandling.yml                # YAML configuration
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
```

## Feature Matrix

| Controller | Features Demonstrated |
|-------------|------------------------|
| **BasicErrors** | Zero-config handling, default error codes |
| **Validation** | DataAnnotations, `fieldErrors` format, OverrideModelStateValidation |
| **Attributes** | `[ResponseErrorCode]`, `[ResponseStatus]`, `[ResponseErrorProperty]` |
| **ConfigDriven** | YAML-based HTTP status/code/message mappings |
| **ProblemDetails** | RFC 9457 Problem Details format |

## API Endpoints

### Basic Errors Controller

Zero-configuration exception handling.

| Endpoint | Description | Response |
|----------|-------------|----------|
| `GET /api/basicerrors/generic` | Generic exception | 500 INTERNAL_ERROR |
| `GET /api/basicerrors/invalid-operation` | Invalid operation | 400 INVALID_OPERATION |
| `GET /api/basicerrors/argument-null` | Argument null | 400 ARGUMENT_NULL |
| `GET /api/basicerrors/key-not-found` | Key not found | 404 KEY_NOT_FOUND |
| `GET /api/basicerrors/timeout` | Timeout | 500 TIMEOUT |

### Validation Controller

DataAnnotations validation with structured `fieldErrors`.

| Endpoint | Description | Request |
|----------|-------------|---------|
| `POST /api/validation/users` | Create user validation | `{ "name": "...", "email": "..." }` |
| `POST /api/validation/transfer` | Transfer validation | `{ "amount": 100 }` |

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
| `GET /api/configdriven/database-timeout` | DatabaseTimeoutException | 503 | DATABASE_TIMEOUT |
| `GET /api/configdriven/service-unavailable` | ServiceUnavailableException | 503 | SERVICE_UNAVAILABLE |
| `GET /api/configdriven/rate-limit` | RateLimitExceededException | 429 | RATE_LIMIT_EXCEEDED |

### Problem Details Controller

RFC 9457 Problem Details format.

| Endpoint | Description | Format |
|----------|-------------|--------|
| `GET /api/problemdetails/basic` | Problem Details response | RFC 9457 |
| `GET /api/problemdetails/with-extension` | With custom properties | RFC 9457 + extensions |

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
  "status": 404
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
  "type": "VALIDATION_ERROR",
  "detail": "One or more validation errors occurred",
  "status": 400,
  "fieldErrors": [
    {
      "property": "email",
      "message": "EMAIL_FORMAT_INVALID",
      "code": "EMAIL_FORMAT_INVALID"
    },
    {
      "property": "name",
      "message": "Name must be at least 2 characters",
      "code": "TOO_SHORT"
    }
  ]
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
  "detail": "User 12345 does not exist",
  "status": 404,
  "userId": "12345"
}
```

### Config-Driven Error

```bash
curl http://localhost:5000/api/configdriven/database-timeout
```

**Response:**
```json
{
  "type": "DATABASE_TIMEOUT",
  "detail": "A database operation timed out. Please try again.",
  "status": 503,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "path": "/api/configdriven/database-timeout"
}
```

### Problem Details Format

```bash
curl http://localhost:5000/api/problemdetails/basic
```

**Response:**
```json
{
  "type": "https://ahmedv20.github.io/error-handling-dotnet/errors/business-rule-violation",
  "title": "Business Rule Violation",
  "detail": "The request violates a business rule",
  "status": 400,
  "instance": "/api/problemdetails/basic"
}
```

## Custom Components

### InfrastructureExceptionHandler

Custom handler for infrastructure exceptions:

```csharp
public class InfrastructureExceptionHandler : AbstractApiExceptionHandler
{
    public override int Order => 50;

    public override bool CanHandle(Exception exception)
        => exception is DatabaseTimeoutException
            or ServiceUnavailableException
            or RateLimitExceededException;

    public override ApiErrorResponse Handle(Exception exception)
    {
        // Maps to configured HTTP status, code, and message from YAML
        return base.Handle(exception);
    }
}
```

### RequestMetadataCustomizer

Adds request metadata to all error responses:

```csharp
public class RequestMetadataCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("traceId", Activity.Current?.Id ?? Guid.NewGuid().ToString());
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            response.AddProperty("path", httpContext.Request.Path);
        }
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
