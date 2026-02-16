# OpenTelemetry Distributed Tracing

ErrorLens automatically creates `Activity` spans via `System.Diagnostics.Activity` when exceptions are handled. There are zero new NuGet dependencies — it uses runtime-provided APIs only. When no `ActivityListener` is subscribed, there is zero overhead.

## How It Works

The static class `ErrorHandlingActivitySource` exposes the activity source used for tracing:

- `ActivitySourceName` = `"ErrorLens.ErrorHandling"`
- `Source` — the shared `ActivitySource` instance

When `ErrorHandlingFacade.HandleException()` runs, an activity named `"ErrorLens.HandleException"` is started. If `activity?.IsAllDataRequested == true`, the following tags are set:

| Tag | Description |
|-----|-------------|
| `error.code` | The ErrorLens error code |
| `error.type` | The exception type name |
| `http.response.status_code` | The HTTP status code returned |

An exception event is added using OTel semantic conventions:

| Event Attribute | Description |
|-----------------|-------------|
| `exception.type` | Fully qualified exception type |
| `exception.message` | Exception message |
| `exception.stacktrace` | Exception stack trace |

The activity status is set to `Error`.

## Setup

Wire the ErrorLens activity source into your OpenTelemetry configuration:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("ErrorLens.ErrorHandling")  // Subscribe to ErrorLens spans
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });
```

## Console Exporter for Debugging

During development, use the console exporter to see spans in your terminal:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("ErrorLens.ErrorHandling")
            .AddConsoleExporter();
    });
```

Example output:

```
Activity.TraceId:   a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4
Activity.SpanId:    1a2b3c4d5e6f7a8b
Activity.DisplayName: ErrorLens.HandleException
Activity.Tags:
    error.code: USER_NOT_FOUND
    error.type: UserNotFoundException
    http.response.status_code: 404
Activity.Events:
    exception [exception.type=UserNotFoundException, exception.message=User abc-123 not found]
Activity.Status: Error
```

## No Listener, No Overhead

When no `ActivityListener` is registered for the `"ErrorLens.ErrorHandling"` source, the `Activity.StartActivity()` call returns `null` and all tag-setting and event-recording code is skipped. This means tracing adds no measurable overhead in environments where it is not configured.
