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
| `NoLogging` | No exception logging at all |
| `MessageOnly` | Log exception message only (default) |
| `WithStacktrace` | Log full exception including stack trace |

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
public class IgnoreNotFoundFilter : ILoggingFilter
{
    public bool ShouldLog(Exception exception, HttpStatusCode statusCode)
    {
        // Don't log 404 errors
        return statusCode != HttpStatusCode.NotFound;
    }
}
```
