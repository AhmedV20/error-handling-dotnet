using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles ModelStateValidationException produced when [ApiController]
/// automatic model validation is intercepted by ErrorLens.
/// Converts ModelStateDictionary errors into structured fieldErrors responses.
/// </summary>
public class ModelStateValidationExceptionHandler : AbstractApiExceptionHandler
{
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly ErrorHandlingOptions _options;

    public ModelStateValidationExceptionHandler(
        IErrorCodeMapper errorCodeMapper,
        IErrorMessageMapper errorMessageMapper,
        IOptions<ErrorHandlingOptions> options)
    {
        _errorCodeMapper = errorCodeMapper;
        _errorMessageMapper = errorMessageMapper;
        _options = options.Value;
    }

    /// <inheritdoc />
    public override int Order => 90;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is ModelStateValidationException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        var modelStateException = (ModelStateValidationException)exception;
        var modelState = modelStateException.ModelState;

        var response = new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.ValidationFailed,
            "Validation failed");

        foreach (var entry in modelState)
        {
            var fieldName = entry.Key;
            var errors = entry.Value;

            if (errors == null || errors.Errors.Count == 0)
                continue;

            foreach (var error in errors.Errors)
            {
                var message = !string.IsNullOrEmpty(error.ErrorMessage)
                    ? error.ErrorMessage
                    : error.Exception?.Message ?? "Validation failed";

                // Try to determine validation type from the error message
                var validationType = InferValidationType(message);
                var fieldKey = $"{fieldName}.{validationType}";
                var defaultCode = MapValidationTypeToCode(validationType);

                var code = _errorCodeMapper.GetErrorCode(fieldKey, defaultCode);
                var resolvedMessage = _errorMessageMapper.GetErrorMessage(fieldKey, defaultCode, message);

                var camelField = ToCamelCase(fieldName);

                if (string.IsNullOrEmpty(fieldName) || fieldName == "$")
                {
                    // Global error (class-level or parse error)
                    response.AddGlobalError(new ApiGlobalError(code, resolvedMessage));
                }
                else
                {
                    var fieldError = new ApiFieldError(
                        code,
                        camelField,
                        resolvedMessage,
                        errors.RawValue,
                        _options.AddPathToError ? camelField : null);

                    response.AddFieldError(fieldError);
                }
            }
        }

        if (_options.HttpStatusInJsonResponse)
        {
            response.Status = (int)HttpStatusCode.BadRequest;
        }

        return response;
    }

    private static string InferValidationType(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "Validation";

        var msg = errorMessage.ToLowerInvariant();

        if (msg.Contains("required") || msg.Contains("is required"))
            return "Required";
        if (msg.Contains("email") && (msg.Contains("invalid") || msg.Contains("valid")))
            return "EmailAddress";
        if (msg.Contains("minimum length") || msg.Contains("maximum length") || msg.Contains("must be a string"))
            return "StringLength";
        if (msg.Contains("between") && (msg.Contains("must be") || msg.Contains("range")))
            return "Range";
        if (msg.Contains("must be greater") || msg.Contains("too low"))
            return "Range";
        if (msg.Contains("must be less") || msg.Contains("too high"))
            return "Range";
        if (msg.Contains("regular expression") || msg.Contains("pattern"))
            return "RegularExpression";
        if (msg.Contains("url") && msg.Contains("valid"))
            return "Url";
        if (msg.Contains("credit card"))
            return "CreditCard";
        if (msg.Contains("json") || msg.Contains("invalid") && msg.Contains("literal"))
            return "Json";

        return "Validation";
    }

    private static string MapValidationTypeToCode(string validationType)
    {
        return validationType switch
        {
            "Required" => DefaultErrorCodes.RequiredNotNull,
            "StringLength" => DefaultErrorCodes.InvalidSize,
            "Range" => DefaultErrorCodes.ValueOutOfRange,
            "EmailAddress" => DefaultErrorCodes.InvalidEmail,
            "RegularExpression" => DefaultErrorCodes.InvalidPattern,
            "Url" => DefaultErrorCodes.InvalidUrl,
            "CreditCard" => DefaultErrorCodes.InvalidCreditCard,
            "Json" => "JSON_PARSE_ERROR",
            _ => DefaultErrorCodes.ValidationFailed
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Handle dotted paths like "Address.ZipCode"
        var parts = name.Split('.');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
                parts[i] = char.ToLowerInvariant(parts[i][0]) + parts[i][1..];
        }

        return string.Join(".", parts);
    }
}
