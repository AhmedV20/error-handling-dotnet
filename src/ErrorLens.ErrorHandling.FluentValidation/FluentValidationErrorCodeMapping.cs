using ErrorLens.ErrorHandling.Configuration;

namespace ErrorLens.ErrorHandling.FluentValidation;

/// <summary>
/// Maps FluentValidation default validator error codes to ErrorLens DefaultErrorCodes constants.
/// </summary>
internal static class FluentValidationErrorCodeMapping
{
    private static readonly Dictionary<string, string> Mappings = new()
    {
        ["NotNullValidator"] = DefaultErrorCodes.RequiredNotNull,
        ["NotEmptyValidator"] = DefaultErrorCodes.RequiredNotEmpty,
        ["EmailValidator"] = DefaultErrorCodes.InvalidEmail,
        ["LengthValidator"] = DefaultErrorCodes.InvalidSize,
        ["MinimumLengthValidator"] = DefaultErrorCodes.InvalidSize,
        ["MaximumLengthValidator"] = DefaultErrorCodes.InvalidSize,
        ["LessThanValidator"] = DefaultErrorCodes.InvalidMax,
        ["LessThanOrEqualValidator"] = DefaultErrorCodes.InvalidMax,
        ["GreaterThanValidator"] = DefaultErrorCodes.InvalidMin,
        ["GreaterThanOrEqualValidator"] = DefaultErrorCodes.InvalidMin,
        ["RegularExpressionValidator"] = DefaultErrorCodes.InvalidPattern,
        ["CreditCardValidator"] = DefaultErrorCodes.InvalidCreditCard,
        ["InclusiveBetweenValidator"] = DefaultErrorCodes.ValueOutOfRange,
        ["ExclusiveBetweenValidator"] = DefaultErrorCodes.ValueOutOfRange,
    };

    /// <summary>
    /// Maps a FluentValidation error code to a DefaultErrorCodes constant.
    /// If the code is found in the mapping dictionary, returns the mapped code.
    /// If not found, returns the error code as-is (preserving user-defined custom codes
    /// and unknown validator names).
    /// </summary>
    /// <param name="errorCode">The FluentValidation error code (typically the validator class name).</param>
    /// <returns>The mapped error code or the original code if no mapping exists.</returns>
    internal static string MapErrorCode(string errorCode)
    {
        return Mappings.TryGetValue(errorCode, out var mapped) ? mapped : errorCode;
    }
}
