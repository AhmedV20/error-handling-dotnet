using System.Net;
using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.ProblemDetails;

/// <summary>
/// Factory interface for creating RFC 9457 Problem Details responses.
/// </summary>
public interface IProblemDetailFactory
{
    /// <summary>
    /// Creates a Problem Details response from an API error response.
    /// </summary>
    /// <param name="apiError">The API error response to convert.</param>
    /// <returns>A Problem Details response.</returns>
    ProblemDetailResponse CreateFromApiError(ApiErrorResponse apiError);

    /// <summary>
    /// Creates a Problem Details response directly from an exception.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="statusCode">The HTTP status code to use.</param>
    /// <returns>A Problem Details response.</returns>
    ProblemDetailResponse CreateFromException(Exception exception, HttpStatusCode statusCode);
}
