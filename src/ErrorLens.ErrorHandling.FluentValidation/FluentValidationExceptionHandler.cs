using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Internal;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.FluentValidation;

/// <summary>
/// Handles <see cref="ValidationException"/> from FluentValidation and maps
/// validation failures to structured <see cref="ApiErrorResponse"/> with field and global errors.
/// </summary>
public class FluentValidationExceptionHandler : IApiExceptionHandler
{
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly ErrorHandlingOptions _options;
    private readonly FluentValidationOptions _fluentOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationExceptionHandler"/> class.
    /// </summary>
    /// <param name="errorCodeMapper">The error code mapper for resolving error codes.</param>
    /// <param name="errorMessageMapper">The error message mapper for resolving error messages.</param>
    /// <param name="options">The core error handling options.</param>
    /// <param name="fluentOptions">The FluentValidation-specific options.</param>
    public FluentValidationExceptionHandler(
        IErrorCodeMapper errorCodeMapper,
        IErrorMessageMapper errorMessageMapper,
        IOptions<ErrorHandlingOptions> options,
        IOptions<FluentValidationOptions> fluentOptions)
    {
        _errorCodeMapper = errorCodeMapper;
        _errorMessageMapper = errorMessageMapper;
        _options = options.Value;
        _fluentOptions = fluentOptions.Value;
    }

    /// <inheritdoc />
    public int Order => 110;

    /// <inheritdoc />
    public bool CanHandle(Exception exception)
    {
        return exception is ValidationException;
    }

    /// <inheritdoc />
    public ApiErrorResponse Handle(Exception exception)
    {
        if (exception is not ValidationException validationException)
            throw new InvalidOperationException(
                $"Cannot handle exception of type {exception.GetType()}. Call CanHandle first.");

        var topLevelMessage = _options.BuiltInMessages.TryGetValue(DefaultErrorCodes.ValidationFailed, out var custom)
            ? custom
            : "Validation failed";

        var response = new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.ValidationFailed,
            topLevelMessage);

        var errors = validationException.Errors?.ToList();

        if (errors == null || errors.Count == 0)
        {
            return response;
        }

        foreach (var failure in errors)
        {
            if (!_fluentOptions.IncludeSeverities.Contains(failure.Severity))
                continue;

            if (string.IsNullOrEmpty(failure.PropertyName))
            {
                // Object-level / global error
                var mappedCode = FluentValidationErrorCodeMapping.MapErrorCode(failure.ErrorCode);
                var globalCode = _errorCodeMapper.GetErrorCode(failure.ErrorCode, mappedCode);
                var globalMessage = _errorMessageMapper.GetErrorMessage(
                    failure.ErrorCode, mappedCode, failure.ErrorMessage);

                response.AddGlobalError(new ApiGlobalError(globalCode, globalMessage));
            }
            else
            {
                // Field-level error
                MapFieldError(response, failure);
            }
        }

        if (_options.HttpStatusInJsonResponse)
        {
            response.Status = (int)HttpStatusCode.BadRequest;
        }

        return response;
    }

    private void MapFieldError(ApiErrorResponse response, ValidationFailure failure)
    {
        var mappedCode = FluentValidationErrorCodeMapping.MapErrorCode(failure.ErrorCode);

        var fieldKey = $"{failure.PropertyName}.{failure.ErrorCode}";
        var code = _errorCodeMapper.GetErrorCode(fieldKey, mappedCode);
        var message = _errorMessageMapper.GetErrorMessage(
            fieldKey, mappedCode, failure.ErrorMessage);

        var camelProperty = StringUtils.ToCamelCase(failure.PropertyName);

        var rejectedValue = _options.IncludeRejectedValues ? failure.AttemptedValue : null;
        var path = _options.AddPathToError ? camelProperty : null;

        var fieldError = new ApiFieldError(code, camelProperty, message, rejectedValue, path);
        response.AddFieldError(fieldError);
    }
}
