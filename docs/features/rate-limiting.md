# Rate Limiting Integration

ErrorLens provides structured error responses for ASP.NET Core rate limiting rejections. Available on .NET 7+ only (gated behind `#if NET7_0_OR_GREATER`).

## Interface

```csharp
public interface IRateLimitResponseWriter
{
    Task WriteRateLimitResponseAsync(
        HttpContext context,
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

## Applying Rate Limiting

### For API Controllers

Use the `[EnableRateLimiting]` attribute on your controller or individual actions:

```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]  // Apply to entire controller
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new[] { "Product1", "Product2" });

    [HttpGet("{id}")]
    public IActionResult GetById(int id) => Ok(new Product { Id = id });
}
```

Or apply to specific actions only:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    // No rate limiting
    public IActionResult GetAll() => Ok(new[] { "Order1" });

    [HttpPost]
    [EnableRateLimiting("api")]  // Only this endpoint
    public IActionResult Create([FromBody] CreateOrderRequest request)
        => CreatedAtAction(nameof(GetById), new { id = 1 }, request);
}
```

### For Minimal APIs

Use the `.RequireRateLimiting()` extension method:

```csharp
// Apply to specific endpoint
app.MapGet("/products", () => Results.Ok(new[] { "Product1", "Product2" }))
    .RequireRateLimiting("api");

// Apply to group
var limitedApi = app.MapGroup("/api/orders")
    .RequireRateLimiting("api");

limitedApi.MapGet("/", () => Results.Ok(new[] { "Order1" }));
limitedApi.MapPost("/", (Order order) => Results.Created($"/api/orders/{order.Id}", order));
```

## Multiple Rate Limit Policies

You can define different policies for different endpoint types:

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Strict policy for expensive operations
    options.AddFixedWindowLimiter("strict", limiter =>
    {
        limiter.PermitLimit = 2;
        limiter.Window = TimeSpan.FromSeconds(30);
    });

    // Relaxed policy for read operations
    options.AddFixedWindowLimiter("relaxed", limiter =>
    {
        limiter.PermitLimit = 100;
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

Then apply different policies:

```csharp
// Controllers
[HttpGet]
[EnableRateLimiting("relaxed")]  // 100 requests/minute
public IActionResult GetAll() { }

[HttpPost]
[EnableRateLimiting("strict")]  // 2 requests/30 seconds
public IActionResult Create([FromBody] Request request) { }

// Minimal APIs
app.MapGet("/products", () => Results.Ok(products))
    .RequireRateLimiting("relaxed");

app.MapPost("/products", (Product p) => Results.Created($"/products/{p.Id}", p))
    .RequireRateLimiting("strict");
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
