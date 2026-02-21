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
}
