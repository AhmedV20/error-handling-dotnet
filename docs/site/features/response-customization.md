# Response Customization

Add custom properties to all error responses using `IApiErrorResponseCustomizer`. This is useful for adding request metadata, tracing information, or any additional context to your error responses.

## Creating a Customizer

Implement the `IApiErrorResponseCustomizer` interface:

```csharp
using ErrorLens.ErrorHandling.Services;
using ErrorLens.ErrorHandling.Models;

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

## Registering a Customizer

Register your customizer with the dependency injection container:

```csharp
// Register IHttpContextAccessor if not already registered
builder.Services.AddHttpContextAccessor();

// Register your customizer
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();

// Add ErrorLens
builder.Services.AddErrorHandling();
```

## Response Example

After customization, all error responses include your extra properties:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "User not found",
  "traceId": "0HNL2K9J4K2L9",
  "timestamp": "2026-02-17T10:30:00.0000000Z",
  "path": "/api/users/123"
}
```

## Multiple Customizers

You can register multiple customizers. They execute in the order they were registered:

```csharp
builder.Services.AddHttpContextAccessor();

// Customizers execute in this order:
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();
builder.Services.AddErrorResponseCustomizer<TimestampCustomizer>();
builder.Services.AddErrorResponseCustomizer<UserContextCustomizer>();
builder.Services.AddErrorResponseCustomizer<ServerInfoCustomizer>();
```

Each customizer can add its own properties:

```csharp
public class TraceIdCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("traceId", Guid.NewGuid().ToString());
    }
}

public class TimestampCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
    }
}
```

## Use Cases

### Distributed Tracing

Add correlation IDs and trace information:

```csharp
public class TracingCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            response.AddProperty("traceId", activity.TraceId.ToString());
            response.AddProperty("spanId", activity.SpanId.ToString());
            response.AddProperty("parentId", activity.ParentSpanId.ToString());
        }
    }
}
```

### Request Context

Add information about the incoming request:

```csharp
public class RequestContextCustomizer : IApiErrorResponseCustomizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextCustomizer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Customize(ApiErrorResponse response)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var request = context.Request;

        response.AddProperty("path", request.Path.Value);
        response.AddProperty("method", request.Method);
        response.AddProperty("queryString", request.QueryString.Value);
        response.AddProperty("contentType", request.ContentType);
    }
}
```

### User Context

Add user information for authentication/authorization scenarios:

```csharp
public class UserContextCustomizer : IApiErrorResponseCustomizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextCustomizer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Customize(ApiErrorResponse response)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            response.AddProperty("userId", user.FindFirst("sub")?.Value);
            response.AddProperty("userName", user.FindFirst("name")?.Value);
            response.AddProperty("tenantId", user.FindFirst("tenant_id")?.Value);
        }
    }
}
```

### Server Information

Add server details for debugging:

```csharp
public class ServerInfoCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        response.AddProperty("instance", Environment.MachineName);
        response.AddProperty("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown");
        response.AddProperty("processId", Environment.ProcessId);
    }
}
```

### Debug Information (Development Only)

Add extra debugging details in development:

```csharp
public class DevelopmentDebugCustomizer : IApiErrorResponseCustomizer
{
    private readonly IWebHostEnvironment _env;

    public DevelopmentDebugCustomizer(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void Customize(ApiErrorResponse response)
    {
        if (_env.IsDevelopment())
        {
            response.AddProperty("environment", "Development");
            response.AddProperty("debugMode", true);
        }
    }
}
```

## Complete Example

Here's a complete setup with multiple customizers:

```csharp
using ErrorLens.ErrorHandling.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add required services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// Register customizers
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();
builder.Services.AddErrorResponseCustomizer<UserContextCustomizer>();
builder.Services.AddErrorResponseCustomizer<ServerInfoCustomizer>();

// Add ErrorLens
builder.Services.AddErrorHandling();

var app = builder.Build();

app.UseErrorHandling();
app.MapControllers();

app.Run();
```

Final error response:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [...],
  "traceId": "0HNL2K9J4K2L9",
  "timestamp": "2026-02-17T10:30:00.0000000Z",
  "path": "/api/users",
  "method": "POST",
  "userId": "12345",
  "tenantId": "abc-123",
  "instance": "WEB-SERVER-01",
  "environment": "Production"
}
```

## Configuration vs Customizers

| Approach | Best For |
|----------|----------|
| **Configuration** | Static mappings, error codes, messages |
| **Customizers** | Dynamic per-request data, metadata |

Use customizers when you need to add data that varies per request (like trace IDs, user context, request paths). Use configuration when setting static values (like default error codes or messages).

## See Also

- [Configuration](/guide/configuration) - Configure error codes and messages
- [Custom Handlers](/features/custom-handlers) - Handle specific exceptions
