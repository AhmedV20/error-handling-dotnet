using System.Net;
using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Base class for exception handlers providing common functionality.
/// </summary>
public abstract class AbstractApiExceptionHandler : IApiExceptionHandler
{
    /// <inheritdoc />
    public abstract bool CanHandle(Exception exception);

    /// <inheritdoc />
    public abstract ApiErrorResponse Handle(Exception exception);

    /// <inheritdoc />
    public virtual int Order => 1000;

    /// <summary>
    /// Creates a basic error response with the given parameters.
    /// </summary>
    protected static ApiErrorResponse CreateResponse(
        HttpStatusCode statusCode,
        string code,
        string? message)
    {
        return new ApiErrorResponse(statusCode, code, message);
    }
}
