using System.Text.RegularExpressions;
using ErrorLens.ErrorHandling.Configuration;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Maps exceptions to error codes based on configuration and conventions.
/// </summary>
public class ErrorCodeMapper : IErrorCodeMapper
{
    private static readonly Regex PascalToSnakeCaseRegex = new("(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);
    private readonly ErrorHandlingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCodeMapper"/> class.
    /// </summary>
    /// <param name="options">The error handling options containing code mappings and strategy configuration.</param>
    public ErrorCodeMapper(IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public string GetErrorCode(Exception exception)
    {
        var exceptionType = exception.GetType();
        var typeName = exceptionType.FullName ?? exceptionType.Name;

        // Check configuration for explicit mapping
        if (_options.Codes.TryGetValue(typeName, out var configuredCode))
        {
            return configuredCode;
        }

        // Check superclass hierarchy if enabled
        if (_options.SearchSuperClassHierarchy)
        {
            var baseType = exceptionType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var baseTypeName = baseType.FullName ?? baseType.Name;
                if (_options.Codes.TryGetValue(baseTypeName, out var baseCode))
                {
                    return baseCode;
                }
                baseType = baseType.BaseType;
            }
        }

        // Generate code based on strategy
        return _options.DefaultErrorCodeStrategy switch
        {
            ErrorCodeStrategy.FullQualifiedName => typeName,
            ErrorCodeStrategy.KebabCase => ConvertToKebabCase(exceptionType.Name),
            ErrorCodeStrategy.PascalCase => ConvertToPascalCase(exceptionType.Name),
            ErrorCodeStrategy.DotSeparated => ConvertToDotSeparated(exceptionType.Name),
            _ => ConvertToAllCaps(exceptionType.Name)
        };
    }

    /// <inheritdoc />
    public string GetErrorCode(string fieldSpecificKey, string defaultCode)
    {
        // Check for field-specific override (e.g., "email.Required")
        if (_options.Codes.TryGetValue(fieldSpecificKey, out var fieldCode))
        {
            return fieldCode;
        }

        // Check for code-specific override
        if (_options.Codes.TryGetValue(defaultCode, out var codeOverride))
        {
            return codeOverride;
        }

        return defaultCode;
    }

    /// <summary>
    /// Converts exception class name to ALL_CAPS format.
    /// Example: UserNotFoundException → USER_NOT_FOUND
    /// </summary>
    private static string ConvertToAllCaps(string className)
    {
        // Remove "Exception" suffix if present, but not if it's the entire name
        var name = className;
        if (name.EndsWith("Exception", StringComparison.Ordinal) && name.Length > 9)
        {
            name = name[..^9];
        }
        else if (name == "Exception")
        {
            // Handle base Exception class
            return "INTERNAL_ERROR";
        }

        // Convert PascalCase to SCREAMING_SNAKE_CASE
        var result = PascalToSnakeCaseRegex.Replace(name, "_$1$2");
        var code = result.ToUpperInvariant().TrimStart('_');
        return string.IsNullOrEmpty(code) ? "UNKNOWN_ERROR" : code;
    }

    /// <summary>
    /// Converts exception class name to kebab-case format.
    /// Example: UserNotFoundException → user-not-found
    /// </summary>
    private static string ConvertToKebabCase(string className)
    {
        var name = StripExceptionSuffix(className);
        if (name == null) return "internal-error";

        var result = PascalToSnakeCaseRegex.Replace(name, "-$1$2");
        var code = result.ToLowerInvariant().TrimStart('-');
        return string.IsNullOrEmpty(code) ? "unknown-error" : code;
    }

    /// <summary>
    /// Converts exception class name to PascalCase format (strips "Exception" suffix only).
    /// Example: UserNotFoundException → UserNotFound
    /// </summary>
    private static string ConvertToPascalCase(string className)
    {
        var name = StripExceptionSuffix(className);
        if (name == null) return "InternalError";

        return string.IsNullOrEmpty(name) ? "UnknownError" : name;
    }

    /// <summary>
    /// Converts exception class name to dot-separated lowercase format.
    /// Example: UserNotFoundException → user.not.found
    /// </summary>
    private static string ConvertToDotSeparated(string className)
    {
        var name = StripExceptionSuffix(className);
        if (name == null) return "internal.error";

        var result = PascalToSnakeCaseRegex.Replace(name, ".$1$2");
        var code = result.ToLowerInvariant().TrimStart('.');
        return string.IsNullOrEmpty(code) ? "unknown.error" : code;
    }

    /// <summary>
    /// Strips the "Exception" suffix from a class name. Returns null if the name is exactly "Exception".
    /// </summary>
    private static string? StripExceptionSuffix(string className)
    {
        if (className == "Exception")
            return null;

        if (className.EndsWith("Exception", StringComparison.Ordinal) && className.Length > 9)
            return className[..^9];

        return className;
    }
}
