using System.Net;

namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Interface for mapping exceptions to HTTP status codes.
/// </summary>
public interface IHttpStatusMapper
{
    /// <summary>
    /// Gets the HTTP status code for an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The HTTP status code.</returns>
    HttpStatusCode GetHttpStatus(Exception exception);

    /// <summary>
    /// Gets the HTTP status code for an exception with a default fallback.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="defaultStatus">The default status if no mapping found.</param>
    /// <returns>The HTTP status code.</returns>
    HttpStatusCode GetHttpStatus(Exception exception, HttpStatusCode defaultStatus);
}
