#if NET8_0_OR_GREATER
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.Serialization;
using ErrorLens.ErrorHandling.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Integration;

/// <summary>
/// Exception handler for .NET 8+ using the native IExceptionHandler interface.
/// </summary>
public class ErrorHandlingExceptionHandler : IExceptionHandler
{
    private readonly ErrorHandlingFacade _facade;
    private readonly ErrorHandlingOptions _options;
    private readonly IProblemDetailFactory _problemDetailFactory;

    public ErrorHandlingExceptionHandler(
        ErrorHandlingFacade facade,
        IOptions<ErrorHandlingOptions> options,
        IProblemDetailFactory problemDetailFactory)
    {
        _facade = facade;
        _options = options.Value;
        _problemDetailFactory = problemDetailFactory;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return false;
        }

        var response = _facade.HandleException(exception);

        httpContext.Response.StatusCode = (int)response.HttpStatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ApiErrorResponseConverter(_options.JsonFieldNames) }
        };

        if (_options.UseProblemDetailFormat)
        {
            var problemDetail = _problemDetailFactory.CreateFromApiError(response);
            problemDetail.Instance = httpContext.Request.Path;

            httpContext.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                problemDetail,
                jsonOptions,
                cancellationToken);
        }
        else
        {
            httpContext.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                response,
                jsonOptions,
                cancellationToken);
        }

        return true;
    }
}
#endif
