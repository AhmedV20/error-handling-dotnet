using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Interface for custom exception handlers.
/// Implementations are ordered by the Order property (lower = higher priority).
/// </summary>
public interface IApiExceptionHandler
{
    /// <summary>
    /// Determines if this handler can handle the given exception.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if this handler can handle the exception.</returns>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Handles the exception and produces an error response.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>The error response.</returns>
    ApiErrorResponse Handle(Exception exception);

    /// <summary>
    /// Handler priority. Lower values are executed first.
    /// Default handlers use values 1000+.
    /// </summary>
    int Order { get; }
}
