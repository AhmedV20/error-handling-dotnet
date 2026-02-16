using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Integration;

/// <summary>
/// Shared response writer that caches <see cref="JsonSerializerOptions"/> for reuse across requests.
/// Used by both <see cref="ErrorHandlingMiddleware"/> (.NET 6/7) and
/// ErrorHandlingExceptionHandler (.NET 8+) to eliminate per-request allocations.
/// </summary>
public class ErrorResponseWriter
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ErrorHandlingOptions _options;
    private readonly IProblemDetailFactory _problemDetailFactory;

    public ErrorResponseWriter(
        IOptions<ErrorHandlingOptions> options,
        IProblemDetailFactory problemDetailFactory)
    {
        _options = options.Value;
        _problemDetailFactory = problemDetailFactory;

        // Cached once — JsonFieldNamesOptions is immutable after DI resolution.
        // JsonSerializerOptions becomes thread-safe after first serialization.
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ApiErrorResponseConverter(_options.JsonFieldNames) }
        };
    }

    /// <summary>
    /// Writes the error response to the HTTP response body as JSON.
    /// Uses Problem Details format when configured, otherwise uses the standard format.
    /// </summary>
    /// <param name="httpContext">The HTTP context to write to.</param>
    /// <param name="response">The structured error response.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public async Task WriteResponseAsync(
        HttpContext httpContext,
        ApiErrorResponse response,
        CancellationToken cancellationToken = default)
    {
        if (httpContext.Response.HasStarted)
            return;

        httpContext.Response.StatusCode = (int)response.HttpStatusCode;

        if (_options.UseProblemDetailFormat)
        {
            var problemDetail = _problemDetailFactory.CreateFromApiError(response);
            problemDetail.Instance = httpContext.Request.Path;

            httpContext.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                problemDetail,
                _jsonOptions,
                cancellationToken);
        }
        else
        {
            httpContext.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                response,
                _jsonOptions,
                cancellationToken);
        }
    }
}
