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

| Order | Handler | Purpose |
|-------|---------|---------|
| 50 | PaymentExceptionHandler | Domain-specific |
| 100 | InfrastructureExceptionHandler | Infrastructure |
| 200 | ValidationExceptionHandler | Built-in validation |
| int.MaxValue | DefaultFallbackHandler | Catch-all fallback |

The `DefaultFallbackHandler` always runs last as the catch-all.

## Built-In Handlers

| Handler | Handles | Priority |
|---------|---------|----------|
| `ValidationExceptionHandler` | `ValidationException` | Built-in |
| `BadRequestExceptionHandler` | `BadHttpRequestException` | Built-in |
| `TypeMismatchExceptionHandler` | `FormatException`, `InvalidCastException` | Built-in |
| `JsonExceptionHandler` | `JsonException` | Built-in |
| `DefaultFallbackHandler` | All unhandled exceptions | Fallback |
