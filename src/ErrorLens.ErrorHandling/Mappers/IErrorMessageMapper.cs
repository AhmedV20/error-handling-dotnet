namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Interface for mapping exceptions to error messages.
/// </summary>
public interface IErrorMessageMapper
{
    /// <summary>
    /// Gets the error message for an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The error message, or null to use exception message.</returns>
    string? GetErrorMessage(Exception exception);

    /// <summary>
    /// Gets the error message for a field-specific validation error.
    /// </summary>
    /// <param name="fieldSpecificKey">The field-specific key (e.g., "email.Required").</param>
    /// <param name="defaultCode">The default code for fallback lookup.</param>
    /// <param name="defaultMessage">The default message if no override found.</param>
    /// <returns>The error message.</returns>
    string GetErrorMessage(string fieldSpecificKey, string defaultCode, string defaultMessage);
}
