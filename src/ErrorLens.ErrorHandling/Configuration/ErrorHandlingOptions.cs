using System.Net;
using ErrorLens.ErrorHandling.OpenApi;
using ErrorLens.ErrorHandling.RateLimiting;
using Microsoft.Extensions.Logging;

namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Main configuration options for error handling, bound to "ErrorHandling:" section.
/// </summary>
public class ErrorHandlingOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "ErrorHandling";

    /// <summary>
    /// Enable or disable error handling. Default: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Strategy for generating error codes. Default: AllCaps
    /// </summary>
    public ErrorCodeStrategy DefaultErrorCodeStrategy { get; set; } = ErrorCodeStrategy.AllCaps;

    /// <summary>
    /// Include HTTP status code in JSON response. Default: false
    /// </summary>
    public bool HttpStatusInJsonResponse { get; set; } = false;

    /// <summary>
    /// Search exception base class hierarchy for configuration. Default: false
    /// </summary>
    public bool SearchSuperClassHierarchy { get; set; } = false;

    /// <summary>
    /// Include property path in field errors. Default: true
    /// </summary>
    public bool AddPathToError { get; set; } = true;

    /// <summary>
    /// Include rejected values in validation field errors. Default: true.
    /// Set to false to prevent sensitive user input (e.g., passwords) from being echoed in error responses.
    /// </summary>
    public bool IncludeRejectedValues { get; set; } = true;

    /// <summary>
    /// Override [ApiController] automatic model validation to use ErrorLens structured fieldErrors format.
    /// When false (default), ASP.NET Core's built-in validation response is used.
    /// When true, validation errors are intercepted and returned as ErrorLens fieldErrors.
    /// </summary>
    public bool OverrideModelStateValidation { get; set; } = false;

    /// <summary>
    /// Use RFC 9457 Problem Details format. Default: false
    /// </summary>
    public bool UseProblemDetailFormat { get; set; } = false;

    /// <summary>
    /// Type URI prefix for Problem Details format. Default: "https://example.com/errors/"
    /// </summary>
    public string ProblemDetailTypePrefix { get; set; } = "https://example.com/errors/";

    /// <summary>
    /// Convert error codes to kebab-case for Problem Details type. Default: true
    /// </summary>
    public bool ProblemDetailConvertToKebabCase { get; set; } = true;

    /// <summary>
    /// Exception logging verbosity. Default: MessageOnly
    /// </summary>
    public ExceptionLogging ExceptionLogging { get; set; } = ExceptionLogging.MessageOnly;

    /// <summary>
    /// Exception type to HTTP status code mappings.
    /// Key: Full exception type name (e.g., "System.ArgumentException")
    /// </summary>
    public Dictionary<string, HttpStatusCode> HttpStatuses { get; set; } = new();

    /// <summary>
    /// Exception type or field-specific error code mappings.
    /// Key: Full exception type name or "fieldName.validationType"
    /// </summary>
    public Dictionary<string, string> Codes { get; set; } = new();

    /// <summary>
    /// Exception type or field-specific error message mappings.
    /// Key: Full exception type name or "fieldName.validationType"
    /// </summary>
    public Dictionary<string, string> Messages { get; set; } = new();

    /// <summary>
    /// HTTP status code to log level mappings.
    /// Key: Status code (e.g., "400", "5xx")
    /// </summary>
    public Dictionary<string, LogLevel> LogLevels { get; set; } = new();

    /// <summary>
    /// HTTP status codes that force full stack trace logging.
    /// Values: Status codes (e.g., "5xx", "500")
    /// </summary>
    public HashSet<string> FullStacktraceHttpStatuses { get; set; } = new();

    /// <summary>
    /// Exception types that force full stack trace logging.
    /// Values: Full exception type names
    /// </summary>
    public HashSet<string> FullStacktraceClasses { get; set; } = new();

    /// <summary>
    /// Custom JSON field names for error responses.
    /// </summary>
    public JsonFieldNamesOptions JsonFieldNames { get; set; } = new();

    /// <summary>
    /// Configuration options for rate limiting error responses.
    /// </summary>
    public RateLimitingOptions RateLimiting { get; set; } = new();

    /// <summary>
    /// Custom message for unhandled 5xx server errors. Only applied to 5xx responses;
    /// 4xx exceptions continue to use their actual exception message.
    /// Default: "An unexpected error occurred"
    /// </summary>
    public string FallbackMessage { get; set; } = "An unexpected error occurred";

    /// <summary>
    /// Override messages for built-in error handlers by error code.
    /// Keys are <see cref="DefaultErrorCodes"/> constant values (e.g., "MESSAGE_NOT_READABLE", "TYPE_MISMATCH", "BAD_REQUEST", "VALIDATION_FAILED").
    /// When a key is present, the corresponding handler uses the custom message instead of its hardcoded default.
    /// Default: empty dictionary (all handlers use their hardcoded defaults).
    /// </summary>
    public Dictionary<string, string> BuiltInMessages { get; set; } = new();

    /// <summary>
    /// Configuration options for OpenAPI/Swagger error response schema generation.
    /// </summary>
    public OpenApiOptions OpenApi { get; set; } = new();
}
