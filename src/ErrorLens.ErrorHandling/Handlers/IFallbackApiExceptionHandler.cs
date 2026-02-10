using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Interface for the fallback exception handler.
/// Used when no IApiExceptionHandler can handle the exception.
/// </summary>
public interface IFallbackApiExceptionHandler
{
    /// <summary>
    /// Handles any exception that wasn't handled by specific handlers.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>The error response.</returns>
    ApiErrorResponse Handle(Exception exception);
}
