# Logging

ErrorLens.ErrorHandling provides configurable logging for handled exceptions.

## Exception Logging Verbosity

Control how much exception detail is logged:

```yaml
ErrorHandling:
  ExceptionLogging: WithStacktrace
```

| Value | Description |
|-------|-------------|
| `None` | No exception logging at all |
| `MessageOnly` | Log exception message only (default) |
| `WithStacktrace` | Log full exception including stack trace |

### Output Examples

**`MessageOnly` (default):**

```
warn: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
```

**`WithStacktrace`:**

```
fail: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
      System.Exception: User was not found
         at MyApp.Services.UserService.GetById(Int32 id)
         at MyApp.Controllers.UsersController.Get(Int32 id)
```

## Log Levels Per HTTP Status

Map HTTP status codes or patterns to log levels:

```yaml
ErrorHandling:
  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug        # More specific overrides range pattern
```

Supported log levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.

Pattern matching: Use `4xx` or `5xx` for ranges, or specific codes like `404`, `500`.

## Full Stack Trace Control

Force full stack trace logging for specific HTTP statuses:

```yaml
ErrorHandling:
  FullStacktraceHttpStatuses:
    - 5xx
    - 400
```

Force full stack trace for specific exception types:

```yaml
ErrorHandling:
  FullStacktraceClasses:
    - System.NullReferenceException
    - MyApp.Exceptions.CriticalException
```

## Logging Filter

Implement `ILoggingFilter` to suppress logging for specific exceptions:

```csharp
public class IgnoreNotFoundFilter : ILoggingFilter
{
    public bool ShouldLog(ApiErrorResponse response, Exception exception)
    {
        return response.HttpStatusCode != HttpStatusCode.NotFound;
    }
}
```

Register your filter:

```csharp
builder.Services.AddSingleton<ILoggingFilter, IgnoreNotFoundFilter>();
```

You can register multiple filters — all filters must return `true` for the exception to be logged.
