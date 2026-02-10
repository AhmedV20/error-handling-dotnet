using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Interface for filtering which exceptions get logged.
/// </summary>
public interface ILoggingFilter
{
    /// <summary>
    /// Determines if the exception should be logged.
    /// </summary>
    /// <param name="response">The error response.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>True if the exception should be logged.</returns>
    bool ShouldLog(ApiErrorResponse response, Exception exception);
}
