using System.Net;

namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Interface for extracting HTTP status codes from exception instances.
/// Used to read status from exception properties or attributes.
/// </summary>
public interface IHttpStatusFromExceptionMapper
{
    /// <summary>
    /// Attempts to extract an HTTP status code from the exception instance.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="statusCode">The extracted status code, if found.</param>
    /// <returns>True if a status code was extracted.</returns>
    bool TryGetHttpStatus(Exception exception, out HttpStatusCode statusCode);
}
