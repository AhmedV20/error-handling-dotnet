namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Logging verbosity for exception handling.
/// </summary>
public enum ExceptionLogging
{
    /// <summary>
    /// No logging of exceptions.
    /// </summary>
    None,

    /// <summary>
    /// Log exception message only (no stack trace).
    /// </summary>
    MessageOnly,

    /// <summary>
    /// Log full exception with stack trace.
    /// </summary>
    WithStacktrace
}
