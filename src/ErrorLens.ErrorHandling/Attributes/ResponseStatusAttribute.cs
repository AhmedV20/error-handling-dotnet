using System.Net;

namespace ErrorLens.ErrorHandling.Attributes;

/// <summary>
/// Specifies the HTTP status code for an exception class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ResponseStatusAttribute : Attribute
{
    /// <summary>
    /// The HTTP status code to use for this exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Creates a new ResponseStatusAttribute with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    public ResponseStatusAttribute(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates a new ResponseStatusAttribute with the specified status code as integer.
    /// </summary>
    /// <param name="statusCode">The HTTP status code as integer.</param>
    public ResponseStatusAttribute(int statusCode)
    {
        StatusCode = (HttpStatusCode)statusCode;
    }
}
