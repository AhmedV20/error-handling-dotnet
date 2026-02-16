using Microsoft.Extensions.Localization;

namespace ErrorLens.ErrorHandling.Localization;

/// <summary>
/// Bridges <see cref="IStringLocalizer{TResource}"/> to <see cref="IErrorMessageLocalizer"/>,
/// looking up error codes as resource keys and falling back to the default message when not found.
/// </summary>
/// <typeparam name="TResource">The resource type used for <see cref="IStringLocalizer{T}"/> resolution.</typeparam>
public class StringLocalizerErrorMessageLocalizer<TResource> : IErrorMessageLocalizer
{
    private readonly IStringLocalizer<TResource> _localizer;

    public StringLocalizerErrorMessageLocalizer(IStringLocalizer<TResource> localizer)
    {
        _localizer = localizer;
    }

    /// <inheritdoc />
    public string? Localize(string errorCode, string? defaultMessage)
    {
        if (string.IsNullOrEmpty(errorCode))
            return defaultMessage;

        var localized = _localizer[errorCode];
        return localized.ResourceNotFound ? defaultMessage : localized.Value;
    }

    /// <inheritdoc />
    public string? LocalizeFieldError(string errorCode, string fieldName, string? defaultMessage)
    {
        if (string.IsNullOrEmpty(errorCode))
            return defaultMessage;

        var localized = _localizer[errorCode];
        return localized.ResourceNotFound ? defaultMessage : localized.Value;
    }
}
