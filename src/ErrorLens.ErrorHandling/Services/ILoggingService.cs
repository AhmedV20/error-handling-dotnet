using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Interface for logging exception handling events.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs an exception based on configured settings.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="response">The error response that will be returned.</param>
    void LogException(Exception exception, ApiErrorResponse response);
}
