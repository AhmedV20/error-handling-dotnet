# FullApiSample

A controller-based API sample demonstrating custom exception handlers and response customizers with ErrorLens.ErrorHandling.

## Overview

This sample shows how to extend ErrorLens.ErrorHandling with:
- Custom exception handlers for domain-specific exceptions
- Response customizers to add metadata to all error responses
- Controller-based API with Swagger UI
- Configuration via `appsettings.json`

## Prerequisites

- .NET 8.0 SDK or later

## Running the Sample

```bash
# From the project root
dotnet run --project samples/FullApiSample

# Or navigate to the sample directory first
cd samples/FullApiSample
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

## Project Structure

```
FullApiSample/
├── Controllers/
│   └── UsersController.cs          # User management endpoints
├── Customizers/
│   └── TraceIdCustomizer.cs       # Adds traceId, timestamp, instance
├── Exceptions/
│   ├── BusinessException.cs       # Base business exception
│   ├── UserNotFoundException.cs   # User not found (404)
│   ├── DuplicateEmailException.cs # Email already exists (409)
│   ├── InsufficientFundsException.cs # Payment failure (400)
│   └── InvalidOrderStateException.cs # Invalid state transition (422)
├── Handlers/
│   └── BusinessExceptionHandler.cs # Custom handler for business exceptions
├── appsettings.json                # Configuration
└── Program.cs                      # Startup configuration
```

## Features Demonstrated

| Feature | Description |
|----------|-------------|
| **Custom Exception Handler** | `BusinessExceptionHandler` intercepts all business exceptions with Order 50 (higher priority than defaults) |
| **Response Customizer** | `TraceIdCustomizer` adds `traceId`, `timestamp`, and `instance` to every error response |
| **Domain Exceptions** | Five custom exception types with specific error codes and HTTP status mappings |
| **Validation Errors** | DataAnnotations validation with structured `fieldErrors` |
| **Swagger Integration** | Full OpenAPI documentation |

## API Endpoints

### Users Controller

| Method | Endpoint | Description | Success/Error |
|--------|----------|-------------|----------------|
| GET | `/api/users` | Get all users | 200 OK |
| GET | `/api/users/{id}` | Get user by ID | 200 OK / 404 `USER_NOT_FOUND` |
| POST | `/api/users` | Create new user | 201 Created / 409 `EMAIL_ALREADY_EXISTS` |
| POST | `/api/users/{id}/transfer` | Transfer funds | 200 OK / 404 `USER_NOT_FOUND` / 400 `INSUFFICIENT_FUNDS` |
| POST | `/api/users/orders/{orderId}/state` | Update order state | 200 OK / 422 `INVALID_ORDER_STATE` |

## Example Requests

### Get All Users

```bash
curl http://localhost:5000/api/users
```

**Response (200 OK):**
```json
[
  {
    "id": "user-1",
    "email": "john@example.com",
    "name": "John Doe"
  },
  {
    "id": "user-2",
    "email": "jane@example.com",
    "name": "Jane Smith"
  }
]
```

### Get User by ID (Success)

```bash
curl http://localhost:5000/api/users/user-1
```

**Response (200 OK):**
```json
{
  "id": "user-1",
  "email": "john@example.com",
  "name": "John Doe"
}
```

### User Not Found

```bash
curl http://localhost:5000/api/users/user-999
```

**Response (404 Not Found):**
```json
{
  "code": "USER_NOT_FOUND",
  "message": "User with ID user-999 was not found",
  "status": 404,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "instance": "DESKTOP-ABC123"
}
```

### Create User - Validation Error

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d "{}"
```

**Response (400 Bad Request):**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "status": 400,
  "fieldErrors": [
    {
      "code": "REQUIRED",
      "property": "email",
      "message": "Email is required"
    },
    {
      "code": "REQUIRED",
      "property": "name",
      "message": "Name is required"
    }
  ],
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "instance": "DESKTOP-ABC123"
}
```

### Create User - Duplicate Email

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"email": "john@example.com", "name": "John Doe"}'
```

**Response (409 Conflict):**
```json
{
  "code": "EMAIL_ALREADY_EXISTS",
  "message": "An account with email 'john@example.com' already exists",
  "status": 409,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "instance": "DESKTOP-ABC123"
}
```

### Transfer - Insufficient Funds

```bash
curl -X POST http://localhost:5000/api/users/user-1/transfer \
  -H "Content-Type: application/json" \
  -d '{"amount": 500.00}'
```

**Response (400 Bad Request):**
```json
{
  "code": "INSUFFICIENT_FUNDS",
  "message": "Insufficient funds: requested $500.00, available $100.00",
  "status": 400,
  "required": 500.00,
  "available": 100.00,
  "traceId": "00-4b75c9e7...",
  "timestamp": "2026-02-12T10:30:45.123Z",
  "instance": "DESKTOP-ABC123"
}
```

## Custom Handler Implementation

The `BusinessExceptionHandler` intercepts all business exceptions:

```csharp
public class BusinessExceptionHandler : AbstractApiExceptionHandler
{
    public override int Order => 50; // Higher priority than default handlers

    public override bool CanHandle(Exception exception)
        => exception is BusinessException;

    public override ApiErrorResponse Handle(Exception exception)
    {
        var businessEx = (BusinessException)exception;
        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            "BUSINESS_RULE_VIOLATION",
            businessEx.Message);
    }
}
```

## Response Customizer Implementation

The `TraceIdCustomizer` adds metadata to all error responses:

```csharp
public class TraceIdCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        // Add trace ID for distributed tracing
        response.AddProperty("traceId", Activity.Current?.Id ?? Guid.NewGuid().ToString());

        // Add timestamp
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));

        // Add environment info
        response.AddProperty("instance", Environment.MachineName);
    }
}
```

## Configuration

The sample uses `appsettings.json` for error handling configuration:

```json
{
  "ErrorHandling": {
    "Enabled": true,
    "HttpStatusInJsonResponse": true,
    "DefaultErrorCodeStrategy": "AllCaps",
    "SearchSuperClassHierarchy": true,
    "ExceptionLogging": "WithStacktrace"
  }
}
```

## Learn More

- [Full Documentation](https://ahmedv20.github.io/error-handling-dotnet/current/)
