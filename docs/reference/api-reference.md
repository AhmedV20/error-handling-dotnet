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
services.AddExceptionHandler<THandler>();

// Register response customizer
services.AddErrorResponseCustomizer<TCustomizer>();
```

### ApplicationBuilderExtensions

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
    ProblemDetailResponse CreateFromException(Exception exception, HttpStatusCode statusCode);
}
```

## Attributes

### ResponseErrorCodeAttribute

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ResponseErrorCodeAttribute : Attribute
{
    public string Code { get; }
    public ResponseErrorCodeAttribute(string code);
}
```

### ResponseStatusAttribute

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ResponseStatusAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public ResponseStatusAttribute(HttpStatusCode statusCode);
    public ResponseStatusAttribute(int statusCode);
}
```

### ResponseErrorPropertyAttribute

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
public class ResponseErrorPropertyAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IncludeIfNull { get; set; }
    public ResponseErrorPropertyAttribute();
    public ResponseErrorPropertyAttribute(string name);
}
```
