namespace ErrorLens.ErrorHandling.Localization;

/// <summary>
/// Default no-op implementation of <see cref="IErrorMessageLocalizer"/> that returns the default message unchanged.
/// Registered by default via TryAddSingleton — replaced when the user opts into localization.
/// </summary>
public class NoOpErrorMessageLocalizer : IErrorMessageLocalizer
{
    /// <inheritdoc />
    public string? Localize(string errorCode, string? defaultMessage) => defaultMessage;

    /// <inheritdoc />
    public string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage) => defaultMessage;
}
