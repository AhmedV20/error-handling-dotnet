# Response Customization

Add global properties to all error responses using `IApiErrorResponseCustomizer`.

## Interface

```csharp
public interface IApiErrorResponseCustomizer
{
    void Customize(ApiErrorResponse response);
}
```

## Implementation

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

## Registration

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();
```

## Result

Every error response now includes the custom properties:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "User not found",
  "traceId": "0HN4L2M8V3Q1S:00000001",
  "timestamp": "2026-02-10T12:00:00.0000000Z",
  "path": "/api/users/123"
}
```

## Multiple Customizers

Register multiple customizers — they all run for every error response:

```csharp
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();
builder.Services.AddErrorResponseCustomizer<TimestampCustomizer>();
builder.Services.AddErrorResponseCustomizer<EnvironmentCustomizer>();
```

## Dependency Injection

Customizers support full constructor injection. You can inject any registered service:

```csharp
public class EnvironmentCustomizer : IApiErrorResponseCustomizer
{
    private readonly IWebHostEnvironment _env;

    public EnvironmentCustomizer(IWebHostEnvironment env) => _env = env;

    public void Customize(ApiErrorResponse response)
    {
        if (_env.IsDevelopment())
        {
            response.AddProperty("environment", "Development");
        }
    }
}
```
