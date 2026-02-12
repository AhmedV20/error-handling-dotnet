# Custom Exception Handlers

Implement `IApiExceptionHandler` to provide specialized handling for specific exception types.

## Interface

```csharp
public interface IApiExceptionHandler
{
    /// <summary>
    /// Handler priority. Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns true if this handler can process the exception.
    /// </summary>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Creates the error response for the exception.
    /// </summary>
    ApiErrorResponse Handle(Exception exception);
}
```

## Implementation

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

## Registration

```csharp
// ErrorLens extension method - registers custom exception handler
builder.Services.AddExceptionHandler<InfrastructureExceptionHandler>();
```

## Using AbstractApiExceptionHandler

For convenience, extend the base class:

```csharp
public class PaymentExceptionHandler : AbstractApiExceptionHandler
{
    public override int Order => 50;

    public override bool CanHandle(Exception exception)
        => exception is PaymentException;

    public override ApiErrorResponse Handle(Exception exception)
    {
        var payEx = (PaymentException)exception;
        var response = new ApiErrorResponse(
            HttpStatusCode.PaymentRequired,
            "PAYMENT_FAILED",
            payEx.Message);
        response.AddProperty("transactionId", payEx.TransactionId);
        return response;
    }
}
```

## Handler Ordering

Handlers are executed in order of their `Order` property (lowest first). The first handler whose `CanHandle()` returns `true` processes the exception.

| Order | Handler | Purpose | Default? |
|-------|---------|---------|----------|
| 90 | ModelStateValidationExceptionHandler | `[ApiController]` model validation (requires `OverrideModelStateValidation: true`) | Yes (with config) |
| 100 | ValidationExceptionHandler | Built-in validation | Yes |
| 120 | JsonExceptionHandler | JSON parsing errors | No (opt-in) |
| 130 | TypeMismatchExceptionHandler | Type conversion errors | No (opt-in) |
| 150 | BadRequestExceptionHandler | Bad HTTP requests | Yes |
| 200+ | Your custom handlers | Domain/infrastructure specific | Manual |
| int.MaxValue | DefaultFallbackHandler | Catch-all fallback | Yes |

The `DefaultFallbackHandler` always runs last as the catch-all.

## Built-In Handlers

| Handler | Handles | Priority |
|---------|---------|----------|
| `ModelStateValidationExceptionHandler` | `ModelStateValidationException` (from `[ApiController]`) | 90 (requires `OverrideModelStateValidation: true`) |
| `ValidationExceptionHandler` | `ValidationException` | 100 |
| `BadRequestExceptionHandler` | `BadHttpRequestException` | 150 |
| `TypeMismatchExceptionHandler` | `FormatException`, `InvalidCastException` | Available (not registered by default) |
| `JsonExceptionHandler` | `JsonException` | Available (not registered by default) |
| `DefaultFallbackHandler` | All unhandled exceptions | Fallback |

**Note:** `ModelStateValidationExceptionHandler`, `ValidationExceptionHandler`, and `BadRequestExceptionHandler` are registered by default. `JsonExceptionHandler` and `TypeMismatchExceptionHandler` must be registered manually if needed:

```csharp
builder.Services.AddExceptionHandler<JsonExceptionHandler>();
builder.Services.AddExceptionHandler<TypeMismatchExceptionHandler>();
```
