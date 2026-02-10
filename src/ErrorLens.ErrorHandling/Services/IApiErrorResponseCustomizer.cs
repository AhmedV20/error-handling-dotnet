using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Interface for customizing all error responses.
/// Used to add global properties like timestamps or correlation IDs.
/// </summary>
public interface IApiErrorResponseCustomizer
{
    /// <summary>
    /// Customizes the error response before it is returned.
    /// </summary>
    /// <param name="response">The error response to customize.</param>
    void Customize(ApiErrorResponse response);
}
