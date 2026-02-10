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

        // Add field errors as extensions
        if (apiError.FieldErrors?.Count > 0)
        {
            problemDetail.Extensions["fieldErrors"] = apiError.FieldErrors;
        }

        // Add global errors as extensions
        if (apiError.GlobalErrors?.Count > 0)
        {
            problemDetail.Extensions["globalErrors"] = apiError.GlobalErrors;
        }

        // Add parameter errors as extensions
        if (apiError.ParameterErrors?.Count > 0)
        {
            problemDetail.Extensions["parameterErrors"] = apiError.ParameterErrors;
        }

        // Add custom properties as extensions
        if (apiError.Properties?.Count > 0)
        {
            foreach (var prop in apiError.Properties)
            {
                problemDetail.Extensions[prop.Key] = prop.Value;
            }
        }

        // Add error code as extension for API compatibility
        problemDetail.Extensions["code"] = apiError.Code;

        return problemDetail;
    }

    /// <inheritdoc />
    public ProblemDetailResponse CreateFromException(Exception exception, HttpStatusCode statusCode)
    {
        return new ProblemDetailResponse
        {
            Type = "about:blank",
            Title = GetTitleFromStatusCode(statusCode),
            Status = (int)statusCode,
            Detail = exception.Message,
            Instance = null
        };
    }

    private string BuildTypeUri(string errorCode)
    {
        if (string.IsNullOrEmpty(_options.ProblemDetailTypePrefix))
        {
            return "about:blank";
        }

        var baseUri = _options.ProblemDetailTypePrefix.TrimEnd('/');
        return $"{baseUri}/{errorCode.ToLowerInvariant().Replace('_', '-')}";
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
