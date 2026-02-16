using System.Diagnostics;

namespace ErrorLens.ErrorHandling.Telemetry;

/// <summary>
/// Provides the shared <see cref="ActivitySource"/> used by ErrorLens for OpenTelemetry-compatible distributed tracing.
/// </summary>
public static class ErrorHandlingActivitySource
{
    /// <summary>
    /// The name of the activity source used for telemetry instrumentation.
    /// </summary>
    public const string ActivitySourceName = "ErrorLens.ErrorHandling";

    /// <summary>
    /// The shared <see cref="ActivitySource"/> instance for creating activities.
    /// </summary>
    public static ActivitySource Source { get; } = new(ActivitySourceName);
}
