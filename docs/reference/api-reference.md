# API Reference

## Extension Methods

### ServiceCollectionExtensions

```csharp
// Zero-config setup
services.AddErrorHandling();

// With inline options
services.AddErrorHandling(options => { ... });

// With IConfiguration binding
services.AddErrorHandling(configuration);

// Register custom exception handler
services.AddApiExceptionHandler<THandler>();

// Register response customizer
services.AddErrorResponseCustomizer<TCustomizer>();

// Enable error message localization using IStringLocalizer<TResource>
services.AddErrorHandlingLocalization<TResource>();
```

### Application Builder Extensions

```csharp
// Add error handling middleware
app.UseErrorHandling();
```

### ConfigurationBuilderExtensions

```csharp
// Add YAML configuration file
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Configuration.AddYamlErrorHandling("custom-path.yml", optional: true, reloadOnChange: true);
```

## Configuration

### ErrorHandlingOptions

| Property | Type | Default |
|----------|------|---------|
| `Enabled` | `bool` | `true` |
| `HttpStatusInJsonResponse` | `bool` | `false` |
| `DefaultErrorCodeStrategy` | `ErrorCodeStrategy` | `AllCaps` |
| `SearchSuperClassHierarchy` | `bool` | `false` |
| `AddPathToError` | `bool` | `true` |
| `OverrideModelStateValidation` | `bool` | `false` |
| `UseProblemDetailFormat` | `bool` | `false` |
| `ProblemDetailTypePrefix` | `string` | `https://example.com/errors/` |
| `ProblemDetailConvertToKebabCase` | `bool` | `true` |
| `ExceptionLogging` | `ExceptionLogging` | `MessageOnly` |
| `HttpStatuses` | `Dictionary<string, HttpStatusCode>` | `{}` |
| `Codes` | `Dictionary<string, string>` | `{}` |
| `Messages` | `Dictionary<string, string>` | `{}` |
| `LogLevels` | `Dictionary<string, LogLevel>` | `{}` |
| `FullStacktraceHttpStatuses` | `HashSet<string>` | `{}` |
| `FullStacktraceClasses` | `HashSet<string>` | `{}` |
| `JsonFieldNames` | `JsonFieldNamesOptions` | (defaults) |
| `RateLimiting` | `RateLimitingOptions` | (defaults) |
| `OpenApi` | `OpenApiOptions` | (defaults) |

> **See also:** [Configuration Guide](../guides/configuration.md)

### JsonFieldNamesOptions

| Property | Type | Default |
|----------|------|---------|
| `Code` | `string` | `code` |
| `Message` | `string` | `message` |
| `Status` | `string` | `status` |
| `FieldErrors` | `string` | `fieldErrors` |
| `GlobalErrors` | `string` | `globalErrors` |
| `ParameterErrors` | `string` | `parameterErrors` |
| `Property` | `string` | `property` |
| `RejectedValue` | `string` | `rejectedValue` |
| `Path` | `string` | `path` |
| `Parameter` | `string` | `parameter` |

> **See also:** [JSON Field Names](../features/json-field-names.md)

**Startup Validation**: All `JsonFieldNames` properties are validated at application startup via `ErrorHandlingOptionsValidator`. Null or empty values are rejected with clear error messages, and duplicate field names are detected. This fails-fast behavior prevents misconfiguration from causing runtime errors.

### ErrorCodeStrategy

| Value | Description |
|-------|-------------|
| `AllCaps` | `UserNotFoundException` → `USER_NOT_FOUND` |
| `FullQualifiedName` | `MyApp.UserNotFoundException` |

### ExceptionLogging

| Value | Description |
|-------|-------------|
| `None` | No exception logging |
| `MessageOnly` | Log message only |
| `WithStacktrace` | Log full exception with stack trace |

> **See also:** [Logging Guide](../guides/logging.md)

## Models

### ApiErrorResponse

```csharp
public class ApiErrorResponse
{
    public string Code { get; set; }
    public string? Message { get; set; }
    public int Status { get; set; }
    public List<ApiFieldError>? FieldErrors { get; set; }
    public List<ApiGlobalError>? GlobalErrors { get; set; }
    public List<ApiParameterError>? ParameterErrors { get; set; }
    public Dictionary<string, object?>? Properties { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }

    // Constructors
    public ApiErrorResponse(string code);
    public ApiErrorResponse(string code, string? message);
    public ApiErrorResponse(HttpStatusCode statusCode, string code, string? message);

    // Methods
    public void AddProperty(string name, object? value);
    public void AddFieldError(ApiFieldError fieldError);
    public void AddGlobalError(ApiGlobalError globalError);
    public void AddParameterError(ApiParameterError parameterError);
}
```

### ApiFieldError

```csharp
public class ApiFieldError
{
    public string Code { get; set; }
    public string Property { get; set; }
    public string Message { get; set; }
    public object? RejectedValue { get; set; }
    public string? Path { get; set; }

    // Constructors
    public ApiFieldError(string code, string property, string message);
    public ApiFieldError(string code, string property, string message, object? rejectedValue, string? path = null);
}
```

### ApiGlobalError

```csharp
public class ApiGlobalError
{
    public string Code { get; set; }
    public string Message { get; set; }

    // Constructor
    public ApiGlobalError(string code, string message);
}
```

### ApiParameterError

```csharp
public class ApiParameterError
{
    public string Code { get; set; }
    public string Parameter { get; set; }
    public string Message { get; set; }
    public object? RejectedValue { get; set; }

    // Constructors
    public ApiParameterError(string code, string parameter, string message);
    public ApiParameterError(string code, string parameter, string message, object? rejectedValue);
}
```

## Interfaces

### IApiExceptionHandler

```csharp
public interface IApiExceptionHandler
{
    int Order { get; }
    bool CanHandle(Exception exception);
    ApiErrorResponse Handle(Exception exception);
}
```

> **See also:** [Custom Handlers](../features/custom-handlers.md)

### IFallbackApiExceptionHandler

```csharp
public interface IFallbackApiExceptionHandler
{
    ApiErrorResponse Handle(Exception exception);
}
```

### IApiErrorResponseCustomizer

```csharp
public interface IApiErrorResponseCustomizer
{
    void Customize(ApiErrorResponse response);
}
```

> **See also:** [Response Customization](../features/response-customization.md)

### ILoggingFilter

```csharp
public interface ILoggingFilter
{
    bool ShouldLog(ApiErrorResponse response, Exception exception);
}
```

### IErrorCodeMapper

```csharp
public interface IErrorCodeMapper
{
    string GetErrorCode(Exception exception);
    string GetErrorCode(string fieldSpecificKey, string defaultCode);
}
```

### IErrorMessageMapper

```csharp
public interface IErrorMessageMapper
{
    string? GetErrorMessage(Exception exception);
    string GetErrorMessage(string fieldSpecificKey, string defaultCode, string defaultMessage);
}
```

### IHttpStatusMapper

```csharp
public interface IHttpStatusMapper
{
    HttpStatusCode GetHttpStatus(Exception exception);
    HttpStatusCode GetHttpStatus(Exception exception, HttpStatusCode defaultStatus);
}
```

### IProblemDetailFactory

```csharp
public interface IProblemDetailFactory
{
    ProblemDetailResponse CreateFromApiError(ApiErrorResponse apiError);
}
```

## Attributes

### ResponseErrorCodeAttribute

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ResponseErrorCodeAttribute : Attribute
{
    public string Code { get; }
    public ResponseErrorCodeAttribute(string code);
}
```

### ResponseStatusAttribute

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ResponseStatusAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public ResponseStatusAttribute(HttpStatusCode statusCode);
    public ResponseStatusAttribute(int statusCode);
}
```

### ResponseErrorPropertyAttribute

```csharp
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ResponseErrorPropertyAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IncludeIfNull { get; set; }
    public ResponseErrorPropertyAttribute();
    public ResponseErrorPropertyAttribute(string name);
}
```

> **See also:** [Attributes Guide](../features/attributes.md)

### IErrorMessageLocalizer

```csharp
public interface IErrorMessageLocalizer
{
    string? Localize(string errorCode, string? defaultMessage);
    string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage);
}
```

> **See also:** [Localization](../features/localization.md)

### IRateLimitResponseWriter (.NET 7+)

```csharp
public interface IRateLimitResponseWriter
{
    Task WriteRateLimitResponseAsync(
        HttpContext context,
        RateLimitLease lease,
        CancellationToken cancellationToken = default);
}
```

> **See also:** [Rate Limiting](../features/rate-limiting.md)

## Integration Package Extensions

### OpenApiServiceCollectionExtensions (ErrorLens.ErrorHandling.OpenApi)

```csharp
// Add ErrorLens error schemas to .NET 9+ OpenAPI documentation
services.AddErrorHandlingOpenApi();
services.AddErrorHandlingOpenApi(options => { options.DefaultStatusCodes = new HashSet<int> { 400, 401, 500 }; });
```

### SwaggerServiceCollectionExtensions (ErrorLens.ErrorHandling.Swashbuckle)

```csharp
// Add ErrorLens error schemas to Swashbuckle Swagger documentation
services.AddErrorHandlingSwashbuckle();
services.AddErrorHandlingSwashbuckle(options => { options.DefaultStatusCodes = new HashSet<int> { 400, 401, 500 }; });
```

## Configuration (New in v1.3.0)

### OpenApiOptions

| Property | Type | Default |
|----------|------|---------|
| `DefaultStatusCodes` | `HashSet<int>` | `{400, 404, 500}` |

### RateLimitingOptions

| Property | Type | Default |
|----------|------|---------|
| `ErrorCode` | `string` | `RATE_LIMIT_EXCEEDED` |
| `DefaultMessage` | `string` | `Too many requests. Please try again later.` |
| `IncludeRetryAfterInBody` | `bool` | `true` |
| `UseModernHeaderFormat` | `bool` | `false` |

## Telemetry

### ErrorHandlingActivitySource

```csharp
public static class ErrorHandlingActivitySource
{
    public const string ActivitySourceName = "ErrorLens.ErrorHandling";
    public static ActivitySource Source { get; }
}
```

> **See also:** [Telemetry](../features/telemetry.md)
```
