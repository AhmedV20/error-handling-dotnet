# OpenTelemetry Distributed Tracing

ErrorLens automatically creates `Activity` spans via `System.Diagnostics.Activity` when exceptions are handled. Zero new NuGet dependencies — uses runtime-provided APIs only.

## Setup

Wire the ErrorLens activity source into your OpenTelemetry configuration:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("ErrorLens.ErrorHandling")
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });
```

## Span Details

Each handled exception creates an activity named `ErrorLens.HandleException` with:

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

## Console Exporter for Debugging

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

When no `ActivityListener` is registered for the `"ErrorLens.ErrorHandling"` source, `Activity.StartActivity()` returns `null` and all tag-setting and event-recording code is skipped. Tracing adds zero measurable overhead when not configured.
