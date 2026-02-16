# Configuration

ErrorLens.ErrorHandling supports both JSON (`appsettings.json`) and YAML (`errorhandling.yml`) configuration using the `ErrorHandling` section name.

## JSON Configuration

Configuration is automatically read from `appsettings.json`:

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

## YAML Configuration

Add YAML support with a single line:

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Services.AddErrorHandling(builder.Configuration);
```

```yaml
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  SearchSuperClassHierarchy: true
  ExceptionLogging: WithStacktrace
```

A full YAML template is available at [`errorhandling-template.yml`](../errorhandling-template.yml).

## All Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable error handling globally |
| `HttpStatusInJsonResponse` | `bool` | `false` | Include HTTP status code in JSON body |
| `DefaultErrorCodeStrategy` | `enum` | `AllCaps` | `AllCaps` or `FullQualifiedName` |
| `SearchSuperClassHierarchy` | `bool` | `false` | Search base classes for config matches |
| `AddPathToError` | `bool` | `true` | Include property path in field errors |
| `OverrideModelStateValidation` | `bool` | `false` | Intercept `[ApiController]` validation to use ErrorLens `fieldErrors` format |
| `UseProblemDetailFormat` | `bool` | `false` | Enable RFC 9457 Problem Details format |
| `ProblemDetailTypePrefix` | `string` | `https://example.com/errors/` | Type URI prefix for Problem Details |
| `ProblemDetailConvertToKebabCase` | `bool` | `true` | Convert error codes to kebab-case in type URI |
| `ExceptionLogging` | `enum` | `MessageOnly` | `None`, `MessageOnly`, `WithStacktrace` |
| `JsonFieldNames` | `JsonFieldNamesOptions` | (defaults) | Custom JSON property names for error responses |

## Dictionary Mappings

### HttpStatuses

Map exception types to HTTP status codes:

```yaml
HttpStatuses:
  MyApp.Exceptions.UserNotFoundException: 404
  MyApp.Exceptions.DuplicateEmailException: 409
  MyApp.Exceptions.ForbiddenException: 403
```

### Codes

Map exception types or field validations to error codes:

```yaml
Codes:
  MyApp.Exceptions.UserNotFoundException: USER_NOT_FOUND
  email.Required: EMAIL_IS_REQUIRED
  email.EmailAddress: EMAIL_FORMAT_INVALID
```

### Messages

Map exception types or field validations to custom messages:

```yaml
Messages:
  MyApp.Exceptions.UserNotFoundException: The requested user was not found
  email.Required: A valid email address is required
```

### LogLevels

Map HTTP status codes to log levels:

```yaml
LogLevels:
  4xx: Warning
  5xx: Error
  404: Debug     # Override specific status within a range
```

### FullStacktraceHttpStatuses

Force full stack trace logging for specific HTTP status patterns:

```yaml
FullStacktraceHttpStatuses:
  - 5xx
  - 400
```

### FullStacktraceClasses

Force full stack trace logging for specific exception types:

```yaml
FullStacktraceClasses:
  - System.NullReferenceException
  - MyApp.Exceptions.CriticalException
```

## Configuration Priority

Settings are resolved in this order (highest priority first):

1. **Inline options** (`Action<ErrorHandlingOptions>` in `AddErrorHandling(options => { ... })`) — overrides everything
2. **Configuration binding** (`appsettings.json` or `errorhandling.yml`) — merged into options
3. **Exception attributes** (`[ResponseErrorCode]`, `[ResponseStatus]`) — checked at runtime
4. **Default conventions** (class name → `ALL_CAPS`, built-in HTTP status mappings)

### How It Works

When you use both configuration binding AND inline options:

```csharp
// appsettings.json sets HttpStatusInJsonResponse = true
// Inline option overrides it to false
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = false; // Wins — inline runs AFTER binding
});
```

Inline options run AFTER `IConfiguration` binding, so they always win when both define the same setting.

When using both JSON and YAML, the last file loaded wins (standard ASP.NET Core behavior).

## Custom JSON Field Names

Customize the property names in error responses to match your API conventions:

```yaml
ErrorHandling:
  JsonFieldNames:
    Code: type            # "code" → "type"
    Message: detail       # "message" → "detail"
    Status: statusCode    # "status" → "statusCode"
    FieldErrors: fields   # "fieldErrors" → "fields"
    GlobalErrors: errors  # "globalErrors" → "errors"
    ParameterErrors: params # "parameterErrors" → "params"
    Property: field       # "property" → "field"
    RejectedValue: value  # "rejectedValue" → "value"
    Path: jsonPath        # "path" → "jsonPath"
    Parameter: paramName  # "parameter" → "paramName"
```

> **See also:** [JSON Field Names](../features/json-field-names.md)

## Extending Error Handling

### Custom Exception Handlers

Register your own exception handlers for specialized exception types:

```csharp
builder.Services.AddApiExceptionHandler<InfrastructureExceptionHandler>();
```

> **See also:** [Custom Handlers](../features/custom-handlers.md)

### Response Customizers

Add global properties to all error responses:

```csharp
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();
```

> **See also:** [Response Customization](../features/response-customization.md)

### Localization

Enable error message localization using `IStringLocalizer<TResource>`:

```csharp
builder.Services.AddErrorHandlingLocalization<MySharedResource>();
```

This replaces the default no-op localizer with a bridge to ASP.NET Core's localization system. Error codes are used as resource keys for message lookup.

### OpenAPI Schema Generation

For .NET 9+ projects using `Microsoft.AspNetCore.OpenApi`:

```csharp
builder.Services.AddErrorHandlingOpenApi();
```

For .NET 6-8 projects using Swashbuckle:

```csharp
builder.Services.AddErrorHandlingSwashbuckle();
```

Configure which status codes get error schemas:

```yaml
ErrorHandling:
  OpenApi:
    DefaultStatusCodes:
      - 400
      - 401
      - 404
      - 500
```

> **See also:** [OpenAPI Integration](../features/openapi.md) | [Swashbuckle Integration](../features/swashbuckle.md)

### Rate Limiting

Configure rate limit error responses (.NET 7+):

```yaml
ErrorHandling:
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```

Wire the writer into ASP.NET Core's rate limiter:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices.GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(context.HttpContext, context.Lease, token);
    };
});
```

> **See also:** [Rate Limiting](../features/rate-limiting.md)

### Telemetry

ErrorLens automatically emits OpenTelemetry-compatible traces when handling exceptions. No configuration is required in ErrorLens itself — just subscribe to the activity source in your OpenTelemetry setup:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ErrorLens.ErrorHandling"));
```

> **See also:** [Telemetry](../features/telemetry.md)
