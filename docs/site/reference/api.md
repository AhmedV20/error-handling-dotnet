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

// Enable error message localization
services.AddErrorHandlingLocalization<TResource>();
```

### Application Builder Extensions

```csharp
app.UseErrorHandling();
```

### ConfigurationBuilderExtensions

```csharp
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
| `IncludeRejectedValues` | `bool` | `true` |
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
| `FallbackMessage` | `string` | `An unexpected error occurred` |
| `BuiltInMessages` | `Dictionary<string, string>` | `{}` |
| `JsonFieldNames` | `JsonFieldNamesOptions` | (defaults) |
| `RateLimiting` | `RateLimitingOptions` | (defaults) |
| `OpenApi` | `OpenApiOptions` | (defaults) |

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
| `RetryAfter` | `string` | `retryAfter` |

::: info Startup Validation
All `JsonFieldNames` properties (including `RetryAfter`) are validated at startup. Null/empty values and duplicate names are rejected with clear error messages. Additionally, `ProblemDetailTypePrefix` is validated as a valid absolute URI (or empty), and `RateLimiting.ErrorCode` / `RateLimiting.DefaultMessage` must be non-empty.
:::

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

### Enums

#### ErrorCodeStrategy

| Value | Description |
|-------|-------------|
| `AllCaps` | `UserNotFoundException` → `USER_NOT_FOUND` |
| `FullQualifiedName` | `MyApp.UserNotFoundException` |
| `KebabCase` | `UserNotFoundException` → `user-not-found` |
| `PascalCase` | `UserNotFoundException` → `UserNotFound` |
| `DotSeparated` | `UserNotFoundException` → `user.not.found` |

#### ExceptionLogging

| Value | Description |
|-------|-------------|
| `None` | No exception logging |
| `MessageOnly` | Log message only |
| `WithStacktrace` | Log full exception with stack trace |

### DefaultErrorCodes

Static class containing all built-in error code constants:

**General Errors:**

| Constant | Value |
|----------|-------|
| `InternalServerError` | `INTERNAL_SERVER_ERROR` |
| `ValidationFailed` | `VALIDATION_FAILED` |
| `MessageNotReadable` | `MESSAGE_NOT_READABLE` |
| `TypeMismatch` | `TYPE_MISMATCH` |
| `AccessDenied` | `ACCESS_DENIED` |
| `Unauthorized` | `UNAUTHORIZED` |
| `NotFound` | `NOT_FOUND` |
| `MethodNotAllowed` | `METHOD_NOT_ALLOWED` |
| `BadRequest` | `BAD_REQUEST` |
| `ClientClosed` | `CLIENT_CLOSED` |

**Validation-Specific Codes:**

| Constant | Value |
|----------|-------|
| `RequiredNotNull` | `REQUIRED_NOT_NULL` |
| `RequiredNotBlank` | `REQUIRED_NOT_BLANK` |
| `RequiredNotEmpty` | `REQUIRED_NOT_EMPTY` |
| `InvalidSize` | `INVALID_SIZE` |
| `InvalidEmail` | `INVALID_EMAIL` |
| `InvalidPattern` | `REGEX_PATTERN_VALIDATION_FAILED` |
| `ValueOutOfRange` | `VALUE_OUT_OF_RANGE` |
| `InvalidUrl` | `INVALID_URL` |
| `InvalidCreditCard` | `INVALID_CREDIT_CARD` |
| `InvalidLength` | `INVALID_LENGTH` |
| `InvalidMin` | `VALUE_TOO_LOW` |
| `InvalidMax` | `VALUE_TOO_HIGH` |

**Rate Limiting:**

| Constant | Value |
|----------|-------|
| `RateLimitExceeded` | `RATE_LIMIT_EXCEEDED` |

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

    public ApiErrorResponse(string code);
    public ApiErrorResponse(string code, string? message);
    public ApiErrorResponse(HttpStatusCode statusCode, string code, string? message);

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
}
```

### ApiGlobalError

```csharp
public class ApiGlobalError
{
    public string Code { get; set; }
    public string Message { get; set; }
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

### ILoggingFilter

```csharp
public interface ILoggingFilter
{
    bool ShouldLog(ApiErrorResponse response, Exception exception);
}
```

### ILoggingService

```csharp
public interface ILoggingService
{
    void LogException(Exception exception, ApiErrorResponse response);
}
```

Default implementation: `LoggingService` — logs exceptions using `ILogger` with configurable log levels per HTTP status range. Respects `ILoggingFilter` instances and `ExceptionLogging` option (`None`, `MessageOnly`, `WithStacktrace`).

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

### IErrorMessageLocalizer

```csharp
public interface IErrorMessageLocalizer
{
    string? Localize(string errorCode, string? defaultMessage);
    string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage);
}
```

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

## Integration Package Extensions

### OpenApiServiceCollectionExtensions

```csharp
services.AddErrorHandlingOpenApi();
services.AddErrorHandlingOpenApi(options => { ... });
```

### SwaggerServiceCollectionExtensions

```csharp
services.AddErrorHandlingSwashbuckle();
services.AddErrorHandlingSwashbuckle(options => { ... });
```

### FluentValidationServiceCollectionExtensions

```csharp
// Zero-config setup
services.AddErrorHandlingFluentValidation();

// With severity filtering options
services.AddErrorHandlingFluentValidation(options =>
{
    options.IncludeSeverities.Add(FluentValidation.Severity.Warning);
});
```

## Telemetry

### ErrorHandlingActivitySource

```csharp
public static class ErrorHandlingActivitySource
{
    public const string ActivitySourceName = "ErrorLens.ErrorHandling";
    public static ActivitySource Source { get; }
}
```

## Attributes

### ResponseErrorCodeAttribute

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ResponseErrorCodeAttribute : Attribute
{
    public string Code { get; }
    public ResponseErrorCodeAttribute(string code);
}
```

### ResponseStatusAttribute

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ResponseStatusAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public ResponseStatusAttribute(HttpStatusCode statusCode);
    public ResponseStatusAttribute(int statusCode);
}
```

### ResponseErrorPropertyAttribute

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ResponseErrorPropertyAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IncludeIfNull { get; set; }
    public ResponseErrorPropertyAttribute();
    public ResponseErrorPropertyAttribute(string name);
}
```
