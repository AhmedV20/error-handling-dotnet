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
// The generic parameter THandler specifies your handler type
builder.Services.AddApiExceptionHandler<InfrastructureExceptionHandler>();
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

**Note**: The `AbstractApiExceptionHandler` base class has a default `Order` value of `1000`. Override this property to control when your handler runs in the chain.

## Handler Ordering

Handlers are executed in order of their `Order` property (lowest first). The first handler whose `CanHandle()` returns `true` processes the exception.

| Order | Handler | Purpose | Default? |
|-------|---------|---------|----------|
| 50 | AggregateExceptionHandler | AggregateException unwrapping (single-inner) and re-dispatching | Yes |
| 90 | ModelStateValidationExceptionHandler | `[ApiController]` model validation (requires `OverrideModelStateValidation: true`) | Yes (with config) |
| 100 | ValidationExceptionHandler | Built-in validation | Yes |
| 120 | JsonExceptionHandler | JSON parsing errors | Yes |
| 130 | TypeMismatchExceptionHandler | Type conversion errors (returns generic message) | Yes |
| 150 | BadRequestExceptionHandler | Bad HTTP requests | Yes |
| 200+ | Your custom handlers | Domain/infrastructure specific | Manual |
| int.MaxValue | DefaultFallbackHandler | Catch-all fallback | Yes |

The `DefaultFallbackHandler` always runs last as the catch-all.

## Built-In Handlers

| Handler | Handles | Priority |
|---------|---------|----------|
| `AggregateExceptionHandler` | `AggregateException` (single-inner unwrapping and re-dispatching) | 50 |
| `ModelStateValidationExceptionHandler` | `ModelStateValidationException` (from `[ApiController]`) | 90 (requires `OverrideModelStateValidation: true`) |
| `ValidationExceptionHandler` | `ValidationException` | 100 |
| `JsonExceptionHandler` | `JsonException` | 120 |
| `TypeMismatchExceptionHandler` | `FormatException`, `InvalidCastException` | 130 |
| `BadRequestExceptionHandler` | `BadHttpRequestException` | 150 |
| `DefaultFallbackHandler` | All unhandled exceptions | Fallback |

**Note:** All built-in handlers (`AggregateExceptionHandler`, `ModelStateValidationExceptionHandler`, `ValidationExceptionHandler`, `JsonExceptionHandler`, `TypeMismatchExceptionHandler`, and `BadRequestExceptionHandler`) are registered by default.

The `ModelStateValidationExceptionHandler` only processes exceptions when `OverrideModelStateValidation` is set to `true` in configuration. When `false` (default), ASP.NET Core's built-in model validation response is used instead.
