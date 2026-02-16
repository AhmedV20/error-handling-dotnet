namespace ErrorLens.ErrorHandling.Localization;

/// <summary>
/// Abstraction for resolving localized error messages by error code.
/// </summary>
public interface IErrorMessageLocalizer
{
    /// <summary>
    /// Returns a localized message for the given error code, or <paramref name="defaultMessage"/> if no translation is found.
    /// </summary>
    /// <param name="errorCode">The error code to look up (e.g., "VALIDATION_FAILED").</param>
    /// <param name="defaultMessage">The fallback message if no localized resource is found.</param>
    /// <returns>The localized message, or the default message.</returns>
    string? Localize(string errorCode, string? defaultMessage);

    /// <summary>
    /// Returns a localized message for a field-specific error, using the error code as the resource key.
    /// </summary>
    /// <param name="errorCode">The error code to look up.</param>
    /// <param name="fieldName">The name of the field with the error.</param>
    /// <param name="defaultMessage">The fallback message if no localized resource is found.</param>
    /// <returns>The localized message, or the default message.</returns>
    string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage);
}
