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

A full YAML template is available at [Configuration Template](/documentation#configuration-template).

## All Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable error handling globally |
| `HttpStatusInJsonResponse` | `bool` | `false` | Include HTTP status code in JSON body |
| `DefaultErrorCodeStrategy` | `enum` | `AllCaps` | `AllCaps`, `FullQualifiedName`, `KebabCase`, `PascalCase`, or `DotSeparated` |
| `SearchSuperClassHierarchy` | `bool` | `false` | Search base classes for config matches |
| `AddPathToError` | `bool` | `true` | Include property path in field errors |
| `IncludeRejectedValues` | `bool` | `true` | Include rejected values in validation errors. Set to `false` to prevent sensitive input (e.g., passwords) from being echoed in responses. |
| `OverrideModelStateValidation` | `bool` | `false` | Intercept `[ApiController]` validation to use ErrorLens `fieldErrors` format |
| `UseProblemDetailFormat` | `bool` | `false` | Enable RFC 9457 Problem Details format |
| `ProblemDetailTypePrefix` | `string` | `https://example.com/errors/` | Type URI prefix for Problem Details |
| `ProblemDetailConvertToKebabCase` | `bool` | `true` | Convert error codes to kebab-case in type URI |
| `ExceptionLogging` | `enum` | `MessageOnly` | `None`, `MessageOnly`, `WithStacktrace` |
| `JsonFieldNames` | `JsonFieldNamesOptions` | (defaults) | Custom JSON property names for error responses (11 configurable fields) |
| `FallbackMessage` | `string` | `An unexpected error occurred` | Custom message for 5xx error responses |
| `BuiltInMessages` | `Dictionary<string, string>` | `{}` | Override default messages for built-in handlers |

## HTTP Status Mappings

Map exception types to HTTP status codes:

::: code-group
```json [appsettings.json]
{
  "ErrorHandling": {
    "HttpStatuses": {
      "System.InvalidOperationException": 409,
      "MyApp.Exceptions.UserNotFoundException": 404
    }
  }
}
```
```yaml [errorhandling.yml]
ErrorHandling:
  HttpStatuses:
    System.InvalidOperationException: 409
    MyApp.Exceptions.UserNotFoundException: 404
```
:::

### Add HTTP Status in JSON Response

```json
{
  "ErrorHandling": {
    "HttpStatusInJsonResponse": true
  }
}
```

Result:

```json
{
  "status": 404,
  "code": "USER_NOT_FOUND",
  "message": "Could not find user with id 123"
}
```

## Error Codes

### Error Code Style

```json
{
  "ErrorHandling": {
    "DefaultErrorCodeStrategy": "FullQualifiedName"
  }
}
```

| Strategy | Example |
|----------|---------|
| `AllCaps` (default) | `USER_NOT_FOUND` |
| `FullQualifiedName` | `MyApp.Exceptions.UserNotFoundException` |
| `KebabCase` | `user-not-found` |
| `PascalCase` | `UserNotFound` |
| `DotSeparated` | `user.not.found` |

### Override Error Codes

::: code-group
```json [appsettings.json]
{
  "ErrorHandling": {
    "Codes": {
      "System.InvalidOperationException": "ILLEGAL_OPERATION",
      "email.Required": "EMAIL_IS_REQUIRED",
      "email.EmailAddress": "EMAIL_FORMAT_INVALID"
    }
  }
}
```
```yaml [errorhandling.yml]
ErrorHandling:
  Codes:
    System.InvalidOperationException: ILLEGAL_OPERATION
    email.Required: EMAIL_IS_REQUIRED
    email.EmailAddress: EMAIL_FORMAT_INVALID
```
:::

## Error Messages

### Override Error Messages

```yaml
ErrorHandling:
  Messages:
    MyApp.Exceptions.UserNotFoundException: The user was not found
    email.Required: A valid email address is required
    password.StringLength: Password must be at least 8 characters
```

## Super Class Hierarchy Search

Search base classes when matching configuration:

```json
{
  "ErrorHandling": {
    "SearchSuperClassHierarchy": true,
    "HttpStatuses": {
      "System.InvalidOperationException": 400
    }
  }
}
```

Any exception that extends `InvalidOperationException` will match these settings.

## Adding Extra Properties

### Via `[ResponseErrorProperty]` Attribute

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
[ResponseStatus(HttpStatusCode.NotFound)]
public class UserNotFoundException : Exception
{
    [ResponseErrorProperty("userId")]
    public string UserId { get; }

    public UserNotFoundException(string userId)
        : base($"Could not find user with id {userId}")
    {
        UserId = userId;
    }
}
```

Response:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "Could not find user with id abc-123",
  "userId": "abc-123"
}
```

### Via `IApiErrorResponseCustomizer`

Add properties globally to all error responses:

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
    }
}
```

Register it:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();
```

## Security

### 5xx Safe Message Behavior

All 5xx-class errors automatically return a generic safe message. The default message can be customized via `FallbackMessage`:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.FallbackMessage = "Something went wrong. Please try again later.";
});
```

```json
{
  "code": "INTERNAL_ERROR",
  "message": "Something went wrong. Please try again later."
}
```

::: info
4xx errors preserve their original messages since these are typically user-facing and safe to expose.
:::

### Customizable Built-in Handler Messages

Override the default messages used by built-in exception handlers via `BuiltInMessages`:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "The request body could not be parsed.";
    options.BuiltInMessages["TYPE_MISMATCH"] = "A value has the wrong type.";
    options.BuiltInMessages["BAD_REQUEST"] = "The request is malformed.";
    options.BuiltInMessages["VALIDATION_FAILED"] = "Please fix the errors below.";
});
```

| Key | Handler | Default Message |
|-----|---------|-----------------|
| `MESSAGE_NOT_READABLE` | `JsonExceptionHandler` | `The request body could not be parsed as valid JSON` |
| `TYPE_MISMATCH` | `TypeMismatchExceptionHandler` | `A type conversion error occurred` |
| `BAD_REQUEST` | `BadRequestExceptionHandler` | `Bad request` |
| `VALIDATION_FAILED` | `ValidationExceptionHandler` | *(exception message)* |

### Startup Validation

Configuration is validated at application startup:

- **`JsonFieldNames`** — All 11 properties are validated: null/empty values are rejected and duplicate names are detected
- **`ProblemDetailTypePrefix`** — Must be a valid absolute URI (e.g., `https://example.com/errors/`) or empty
- **`RateLimiting.ErrorCode`** — Must not be null or empty
- **`RateLimiting.DefaultMessage`** — Must not be null or empty

## Configuration Priority

Settings are resolved in this order (highest priority first):

1. **Custom exception handlers** — `IApiExceptionHandler` implementations run first in the pipeline
2. **Inline options** — `Action<ErrorHandlingOptions>` in `AddErrorHandling()`
3. **Configuration binding** — `appsettings.json` or `errorhandling.yml`
4. **Exception attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`
5. **Default conventions** — class name to `ALL_CAPS`, built-in HTTP status mappings

## Integration Configuration

### OpenAPI Schema Generation

```csharp
builder.Services.AddErrorHandlingOpenApi();
```

See [OpenAPI Integration](/features/openapi) | [Swashbuckle Integration](/features/swashbuckle)

### Rate Limiting

```yaml
ErrorHandling:
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```

See [Rate Limiting](/features/rate-limiting)

### Telemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ErrorLens.ErrorHandling"));
```

See [Telemetry](/features/telemetry)
