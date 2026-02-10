using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.Serialization;
using ErrorLens.ErrorHandling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Integration;

/// <summary>
/// Exception handling middleware for .NET 6/7 (and as fallback for .NET 8+).
/// </summary>
public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ErrorHandlingFacade _facade;
    private readonly ErrorHandlingOptions _options;
    private readonly IProblemDetailFactory _problemDetailFactory;

    public ErrorHandlingMiddleware(
        ErrorHandlingFacade facade,
        IOptions<ErrorHandlingOptions> options,
        IProblemDetailFactory problemDetailFactory)
    {
        _facade = facade;
        _options = options.Value;
        _problemDetailFactory = problemDetailFactory;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_options.Enabled)
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = _facade.HandleException(exception);

        context.Response.StatusCode = (int)response.HttpStatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ApiErrorResponseConverter(_options.JsonFieldNames) }
        };

        if (_options.UseProblemDetailFormat)
        {
            var problemDetail = _problemDetailFactory.CreateFromApiError(response);
            problemDetail.Instance = context.Request.Path;

            context.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problemDetail,
                jsonOptions);
        }
        else
        {
            context.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                response,
                jsonOptions);
        }
    }
}
