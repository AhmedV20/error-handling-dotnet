namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Interface for mapping exceptions to error codes.
/// </summary>
public interface IErrorCodeMapper
{
    /// <summary>
    /// Gets the error code for an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The error code.</returns>
    string GetErrorCode(Exception exception);

    /// <summary>
    /// Gets the error code for a field-specific validation error.
    /// </summary>
    /// <param name="fieldSpecificKey">The field-specific key (e.g., "email.Required").</param>
    /// <param name="defaultCode">The default code if no override found.</param>
    /// <returns>The error code.</returns>
    string GetErrorCode(string fieldSpecificKey, string defaultCode);
}
