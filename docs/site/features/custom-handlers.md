# Custom Exception Handlers

Implement `IApiExceptionHandler` to provide specialized handling for specific exception types.

## Interface

```csharp
public interface IApiExceptionHandler
{
    int Order { get; }
    bool CanHandle(Exception exception);
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

::: info
`AbstractApiExceptionHandler` has a default `Order` value of `1000`. Override this property to control when your handler runs in the chain.
:::

## Handler Ordering

Handlers execute in order of their `Order` property (lowest first). The first handler whose `CanHandle()` returns `true` processes the exception.

| Order | Handler | Purpose | Default? |
|-------|---------|---------|----------|
| 50 | `AggregateExceptionHandler` | `AggregateException` unwrapping | Yes |
| 90 | `ModelStateValidationExceptionHandler` | `[ApiController]` model validation | Yes (with config) |
| 100 | `ValidationExceptionHandler` | DataAnnotations validation | Yes |
| 120 | `JsonExceptionHandler` | JSON parsing errors | Yes |
| 130 | `TypeMismatchExceptionHandler` | Type conversion errors | Yes |
| 150 | `BadRequestExceptionHandler` | Bad HTTP requests | Yes |
| 200+ | Your custom handlers | Domain/infrastructure specific | Manual |
| int.MaxValue | `DefaultFallbackHandler` | Catch-all fallback | Yes |

The `DefaultFallbackHandler` always runs last as the catch-all.

## Custom Fallback Handler

Replace the built-in `DefaultFallbackHandler` by implementing `IFallbackApiExceptionHandler`. The fallback handler runs when **no** `IApiExceptionHandler` matches the exception.

```csharp
public class SafeFallbackHandler : IFallbackApiExceptionHandler
{
    private readonly ILogger<SafeFallbackHandler> _logger;

    public SafeFallbackHandler(ILogger<SafeFallbackHandler> logger)
    {
        _logger = logger;
    }

    public ApiErrorResponse Handle(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Type}", exception.GetType().Name);

        var response = new ApiErrorResponse(
            HttpStatusCode.InternalServerError,
            "INTERNAL_SERVER_ERROR",
            "An unexpected error occurred. Please contact support if this persists.");

        // Add a support reference for incident tracking
        response.AddProperty("supportReference", $"ERR-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        return response;
    }
}
```

Register it as a singleton — this replaces the built-in `DefaultFallbackHandler`:

```csharp
builder.Services.AddSingleton<IFallbackApiExceptionHandler, SafeFallbackHandler>();
```

::: tip
Use the fallback handler for global cross-cutting concerns like incident tracking, custom safe messages, or support reference IDs. For exception-type-specific handling, use `IApiExceptionHandler` instead.
:::
