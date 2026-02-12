# Logging

ErrorLens.ErrorHandling provides configurable logging for handled exceptions, allowing you to control verbosity and log levels per HTTP status code.

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
```text
warn: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
```

**`WithStacktrace`:**
```text
fail: ErrorLens.ErrorHandling[0]
      Exception handled: USER_NOT_FOUND - User was not found
      System.Exception: User was not found
         at MyApp.Services.UserService.GetById(Int32 id)
         at MyApp.Controllers.UsersController.Get(Int32 id)
```

**`None`:**
```text
(no output - exception is handled silently)
```

## Log Levels Per HTTP Status

Map HTTP status codes or patterns to log levels:

```yaml
ErrorHandling:
  LogLevels:
    4xx: Warning      # All 4xx errors → Warning
    5xx: Error        # All 5xx errors → Error
    404: Debug        # Override: 404 → Debug (more specific wins)
```

Supported log levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.

Pattern matching: Use `4xx` or `5xx` for ranges, or specific codes like `404`, `500`.

### Example with LogLevels

With the configuration above:
- GET /api/users/999 → 404 → logs at `Debug` level (specific override)
- POST /api/users with invalid body → 400 → logs at `Warning` level (4xx pattern)
- Internal server error → 500 → logs at `Error` level (5xx pattern)

More specific patterns (e.g., `404`) take priority over range patterns (e.g., `4xx`).

## Full Stack Trace Control

Force full stack trace logging for specific HTTP statuses, regardless of the `ExceptionLogging` setting:

```yaml
ErrorHandling:
  FullStacktraceHttpStatuses:
    - 5xx          # All server errors get full stack trace
    - 400          # Specific status code
```

Force full stack trace for specific exception types:

```yaml
ErrorHandling:
  FullStacktraceClasses:
    - System.NullReferenceException
    - MyApp.Exceptions.CriticalException
```

## Default Behavior

Without any logging configuration:

- `ExceptionLogging` defaults to `MessageOnly`
- No specific log level mappings (uses default logger level)
- No forced stack traces

## Logging Filter

Implement `ILoggingFilter` to suppress logging for specific exceptions:

```csharp
using ErrorLens.ErrorHandling.Models;

public class IgnoreNotFoundFilter : ILoggingFilter
{
    public bool ShouldLog(ApiErrorResponse response, Exception exception)
    {
        // Don't log 404 errors
        return response.HttpStatusCode != HttpStatusCode.NotFound;
    }
}
```
