using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.ProblemDetails;

/// <summary>
/// Factory for creating RFC 9457 Problem Details responses.
/// </summary>
public class ProblemDetailFactory : IProblemDetailFactory
{
    private readonly ErrorHandlingOptions _options;

    public ProblemDetailFactory(IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public ProblemDetailResponse CreateFromApiError(ApiErrorResponse apiError)
    {
        var problemDetail = new ProblemDetailResponse
        {
            Type = BuildTypeUri(apiError.Code),
            Title = GetTitle(apiError),
            Status = (int)apiError.HttpStatusCode,
            Detail = apiError.Message,
            Instance = null // Can be set by customizers or middleware
        };

        var fieldNames = _options.JsonFieldNames;

        // Add field errors as extensions (copy list to prevent shared mutable references)
        if (apiError.FieldErrors?.Count > 0)
        {
            problemDetail.Extensions[fieldNames.FieldErrors] = apiError.FieldErrors.ToList();
        }

        // Add global errors as extensions (copy list to prevent shared mutable references)
        if (apiError.GlobalErrors?.Count > 0)
        {
            problemDetail.Extensions[fieldNames.GlobalErrors] = apiError.GlobalErrors.ToList();
        }

        // Add parameter errors as extensions (copy list to prevent shared mutable references)
        if (apiError.ParameterErrors?.Count > 0)
        {
            problemDetail.Extensions[fieldNames.ParameterErrors] = apiError.ParameterErrors.ToList();
        }

        // Add custom properties as extensions (skip keys already set by the library)
        if (apiError.Properties?.Count > 0)
        {
            foreach (var prop in apiError.Properties)
            {
                problemDetail.Extensions.TryAdd(prop.Key, prop.Value);
            }
        }

        // Add error code as extension for API compatibility
        problemDetail.Extensions.TryAdd(fieldNames.Code, apiError.Code);

        return problemDetail;
    }

    private string BuildTypeUri(string errorCode)
    {
        if (string.IsNullOrEmpty(_options.ProblemDetailTypePrefix) || string.IsNullOrEmpty(errorCode))
        {
            return "about:blank";
        }

        var baseUri = _options.ProblemDetailTypePrefix.TrimEnd('/');
        var formattedCode = _options.ProblemDetailConvertToKebabCase
            ? errorCode.ToLowerInvariant().Replace('_', '-')
            : errorCode;
        return $"{baseUri}/{formattedCode}";
    }

    private static string GetTitle(ApiErrorResponse apiError)
    {
        // Use HTTP status reason phrase as title
        return GetTitleFromStatusCode(apiError.HttpStatusCode);
    }

    private static string GetTitleFromStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
            HttpStatusCode.TooManyRequests => "Too Many Requests",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout",
            _ => "Error"
        };
    }
}
