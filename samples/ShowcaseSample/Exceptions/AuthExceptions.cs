using System.Net;
using ErrorLens.ErrorHandling.Attributes;

namespace ShowcaseSample.Exceptions;

// --- Auth Exceptions ---

/// <summary>
/// Thrown when authentication is required but not provided.
/// </summary>
[ResponseErrorCode("UNAUTHORIZED")]
[ResponseStatus(HttpStatusCode.Unauthorized)]
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Authentication is required to access this resource") { }

    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>
/// Thrown when the user lacks permission for the requested operation.
/// </summary>
[ResponseErrorCode("FORBIDDEN")]
[ResponseStatus(HttpStatusCode.Forbidden)]
public class ForbiddenException : Exception
{
    [ResponseErrorProperty("requiredRole")]
    public string? RequiredRole { get; }

    public ForbiddenException(string? requiredRole = null)
        : base("You do not have permission to perform this action")
    {
        RequiredRole = requiredRole;
    }
}
