using ErrorLens.ErrorHandling.Configuration;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Maps exceptions to error messages based on configuration.
/// </summary>
public class ErrorMessageMapper : IErrorMessageMapper
{
    private readonly ErrorHandlingOptions _options;

    public ErrorMessageMapper(IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public string? GetErrorMessage(Exception exception)
    {
        var exceptionType = exception.GetType();
        var typeName = exceptionType.FullName ?? exceptionType.Name;

        // Check configuration for explicit message override
        if (_options.Messages.TryGetValue(typeName, out var configuredMessage))
        {
            return configuredMessage;
        }

        // Check superclass hierarchy if enabled
        if (_options.SearchSuperClassHierarchy)
        {
            var baseType = exceptionType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var baseTypeName = baseType.FullName ?? baseType.Name;
                if (_options.Messages.TryGetValue(baseTypeName, out var baseMessage))
                {
                    return baseMessage;
                }
                baseType = baseType.BaseType;
            }
        }

        // Return exception message as default
        return exception.Message;
    }

    /// <inheritdoc />
    public string GetErrorMessage(string fieldSpecificKey, string defaultCode, string defaultMessage)
    {
        // Check for field-specific override (e.g., "email.Required")
        if (_options.Messages.TryGetValue(fieldSpecificKey, out var fieldMessage))
        {
            return fieldMessage;
        }

        // Check for code-specific override
        if (_options.Messages.TryGetValue(defaultCode, out var codeMessage))
        {
            return codeMessage;
        }

        return defaultMessage;
    }
}
