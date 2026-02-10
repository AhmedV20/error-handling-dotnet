namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Default error codes matching Java reference implementation.
/// </summary>
public static class DefaultErrorCodes
{
    // General errors
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string MessageNotReadable = "MESSAGE_NOT_READABLE";
    public const string TypeMismatch = "TYPE_MISMATCH";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string NotFound = "NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string BadRequest = "BAD_REQUEST";

    // Validation-specific codes
    public const string RequiredNotNull = "REQUIRED_NOT_NULL";
    public const string RequiredNotBlank = "REQUIRED_NOT_BLANK";
    public const string RequiredNotEmpty = "REQUIRED_NOT_EMPTY";
    public const string InvalidSize = "INVALID_SIZE";
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string InvalidPattern = "REGEX_PATTERN_VALIDATION_FAILED";
    public const string ValueOutOfRange = "VALUE_OUT_OF_RANGE";
    public const string InvalidUrl = "INVALID_URL";
    public const string InvalidCreditCard = "INVALID_CREDIT_CARD";
    public const string InvalidLength = "INVALID_LENGTH";
    public const string InvalidMin = "VALUE_TOO_LOW";
    public const string InvalidMax = "VALUE_TOO_HIGH";
}
