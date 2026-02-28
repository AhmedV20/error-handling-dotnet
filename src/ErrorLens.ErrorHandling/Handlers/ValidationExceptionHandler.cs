using System.ComponentModel.DataAnnotations;
using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Internal;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles ValidationException from DataAnnotations validation.
/// </summary>
public class ValidationExceptionHandler : AbstractApiExceptionHandler
{
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly ErrorHandlingOptions _options;

    public ValidationExceptionHandler(
        IErrorCodeMapper errorCodeMapper,
        IErrorMessageMapper errorMessageMapper,
        IOptions<ErrorHandlingOptions> options)
    {
        _errorCodeMapper = errorCodeMapper;
        _errorMessageMapper = errorMessageMapper;
        _options = options.Value;
    }

    /// <inheritdoc />
    public override int Order => 100;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is ValidationException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        if (exception is not ValidationException validationException)
            throw new InvalidOperationException($"Cannot handle exception of type {exception.GetType()}. Call CanHandle first.");

        var topLevelMessage = _options.BuiltInMessages.TryGetValue(DefaultErrorCodes.ValidationFailed, out var custom)
            ? custom
            : validationException.Message;

        var response = new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.ValidationFailed,
            topLevelMessage);

        // Extract field errors from ValidationResult
        if (validationException.ValidationResult != null)
        {
            var result = validationException.ValidationResult;
            var memberNames = result.MemberNames?.ToList() ?? new List<string>();

            if (memberNames.Count > 0)
            {
                foreach (var memberName in memberNames)
                {
                    var fieldKey = $"{memberName}.{GetValidationCode(validationException)}";
                    var defaultCode = GetDefaultValidationCode(validationException);
                    var code = _errorCodeMapper.GetErrorCode(fieldKey, defaultCode);
                    var message = _errorMessageMapper.GetErrorMessage(fieldKey, defaultCode, result.ErrorMessage ?? "Validation failed");

                    var rejectedValue = _options.IncludeRejectedValues ? validationException.Value : null;

                    var fieldError = new ApiFieldError(
                        code,
                        StringUtils.ToCamelCase(memberName),
                        message,
                        rejectedValue,
                        _options.AddPathToError ? StringUtils.ToCamelCase(memberName) : null);

                    response.AddFieldError(fieldError);
                }
            }
            else
            {
                // Global validation error (no specific field)
                var code = GetDefaultValidationCode(validationException);
                response.AddGlobalError(new ApiGlobalError(code, result.ErrorMessage ?? "Validation failed"));
            }
        }

        if (_options.HttpStatusInJsonResponse)
        {
            response.Status = (int)HttpStatusCode.BadRequest;
        }

        return response;
    }

    private static string GetValidationCode(ValidationException exception)
    {
        // Try to determine the validation type from the exception
        return exception.ValidationAttribute?.GetType().Name.Replace("Attribute", "") ?? "Validation";
    }

    private static string GetDefaultValidationCode(ValidationException exception)
    {
        var attributeType = exception.ValidationAttribute?.GetType();

        if (attributeType == null)
        {
            return DefaultErrorCodes.ValidationFailed;
        }

        return attributeType.Name switch
        {
            "RequiredAttribute" => DefaultErrorCodes.RequiredNotNull,
            "StringLengthAttribute" => DefaultErrorCodes.InvalidSize,
            "MaxLengthAttribute" => DefaultErrorCodes.InvalidSize,
            "MinLengthAttribute" => DefaultErrorCodes.InvalidSize,
            "RangeAttribute" => DefaultErrorCodes.ValueOutOfRange,
            "EmailAddressAttribute" => DefaultErrorCodes.InvalidEmail,
            "RegularExpressionAttribute" => DefaultErrorCodes.InvalidPattern,
            "UrlAttribute" => DefaultErrorCodes.InvalidUrl,
            "CreditCardAttribute" => DefaultErrorCodes.InvalidCreditCard,
            _ => DefaultErrorCodes.ValidationFailed
        };
    }
}
