# Rate Limiting Integration

ErrorLens provides structured error responses for ASP.NET Core rate limiting rejections. Available on .NET 7+ only (gated behind `#if NET7_0_OR_GREATER`).

## Interface

```csharp
public interface IRateLimitResponseWriter
{
    ValueTask WriteRateLimitResponseAsync(
        HttpContext httpContext,
        RateLimitLease lease,
        CancellationToken cancellationToken = default);
}
```

`DefaultRateLimitResponseWriter` implements this interface and writes a structured ErrorLens 429 response.

## Setup

Wire the `IRateLimitResponseWriter` into ASP.NET Core's rate limiter:

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter("global", _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, cancellationToken);
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

- `Retry-After` header is set from the lease metadata (seconds until the client can retry)
- When `UseModernHeaderFormat` is enabled, a `RateLimit` header is added using the modern IETF draft format

## Configuration

`RateLimitingOptions` is nested inside `ErrorHandlingOptions.RateLimiting`:

| Option | Default | Description |
|--------|---------|-------------|
| `ErrorCode` | `"RATE_LIMIT_EXCEEDED"` | Error code in the response |
| `DefaultMessage` | `"Too many requests. Please try again later."` | Default error message |
| `IncludeRetryAfterInBody` | `true` | Include `retryAfter` property in the JSON body |
| `UseModernHeaderFormat` | `false` | Use IETF draft `RateLimit` header format |

### Via Code

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.RateLimiting.ErrorCode = "RATE_LIMIT_EXCEEDED";
    options.RateLimiting.DefaultMessage = "Slow down! Try again shortly.";
    options.RateLimiting.IncludeRetryAfterInBody = true;
    options.RateLimiting.UseModernHeaderFormat = true;
});
```

### Via YAML

```yaml
ErrorHandling:
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```

## Localization

Rate limit messages are localized through the same `IErrorMessageLocalizer` pipeline. The `ErrorCode` is used as the resource key for message lookup. See [Localization](localization.md) for setup details.
