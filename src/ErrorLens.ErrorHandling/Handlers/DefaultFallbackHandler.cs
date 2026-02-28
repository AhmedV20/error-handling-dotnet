using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Internal;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Default fallback handler for exceptions not matched by specific handlers.
/// </summary>
public class DefaultFallbackHandler : IFallbackApiExceptionHandler
{
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly IHttpStatusMapper _httpStatusMapper;
    private readonly ErrorHandlingOptions _options;

    public DefaultFallbackHandler(
        IErrorCodeMapper errorCodeMapper,
        IErrorMessageMapper errorMessageMapper,
        IHttpStatusMapper httpStatusMapper,
        IOptions<ErrorHandlingOptions> options)
    {
        _errorCodeMapper = errorCodeMapper;
        _errorMessageMapper = errorMessageMapper;
        _httpStatusMapper = httpStatusMapper;
        _options = options.Value;
    }

    /// <inheritdoc />
    public ApiErrorResponse Handle(Exception exception)
    {
        var metadata = ExceptionMetadataCache.GetMetadata(exception.GetType());

        // Use attribute code if present, otherwise use mapper
        var code = metadata.ErrorCode ?? _errorCodeMapper.GetErrorCode(exception);
        var message = _errorMessageMapper.GetErrorMessage(exception);
        var status = metadata.StatusCode ?? _httpStatusMapper.GetHttpStatus(exception);

        // For 5xx errors, use safe generic message to prevent information disclosure
        if ((int)status >= 500)
        {
            message = _options.FallbackMessage;
        }

        var response = new ApiErrorResponse(status, code, message);

        // Add properties marked with [ResponseErrorProperty]
        foreach (var prop in metadata.Properties)
        {
            var value = prop.Property.GetValue(exception);
            if (value != null || prop.IncludeIfNull)
            {
                response.AddProperty(prop.Name, value);
            }
        }

        // Include HTTP status in JSON if configured
        if (_options.HttpStatusInJsonResponse)
        {
            response.Status = (int)status;
        }

        return response;
    }
}
