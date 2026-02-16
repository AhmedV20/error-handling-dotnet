# Rate Limiting Integration

ErrorLens provides structured error responses for ASP.NET Core rate limiting rejections. Available on .NET 7+ only.

## Setup

Wire the `IRateLimitResponseWriter` into ASP.NET Core's rate limiter:

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
    });

    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, token);
    };
});
```

## Response Format

The writer produces a standard ErrorLens error response with HTTP 429:

```json
{
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 30
}
```

## Headers

- `Retry-After` header is set from the lease metadata
- When `UseModernHeaderFormat` is enabled, a `RateLimit` header is added

## Configuration

::: code-group
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.RateLimiting.ErrorCode = "RATE_LIMIT_EXCEEDED";
    options.RateLimiting.DefaultMessage = "Slow down! Try again shortly.";
    options.RateLimiting.IncludeRetryAfterInBody = true;
    options.RateLimiting.UseModernHeaderFormat = true;
});
```
```yaml [YAML]
ErrorHandling:
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```
:::

| Option | Default | Description |
|--------|---------|-------------|
| `ErrorCode` | `RATE_LIMIT_EXCEEDED` | Error code in the response |
| `DefaultMessage` | `Too many requests. Please try again later.` | Default error message |
| `IncludeRetryAfterInBody` | `true` | Include `retryAfter` property in JSON body |
| `UseModernHeaderFormat` | `false` | Use IETF draft `RateLimit` header format |

## Localization

Rate limit messages are localized through the same `IErrorMessageLocalizer` pipeline. See [Localization](/features/localization) for setup details.
