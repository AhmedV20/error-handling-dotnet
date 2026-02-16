using ErrorLens.ErrorHandling.Configuration;
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
    private readonly ErrorResponseWriter _responseWriter;

    public ErrorHandlingMiddleware(
        ErrorHandlingFacade facade,
        IOptions<ErrorHandlingOptions> options,
        ErrorResponseWriter responseWriter)
    {
        _facade = facade;
        _options = options.Value;
        _responseWriter = responseWriter;
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
        await _responseWriter.WriteResponseAsync(context, response, context.RequestAborted);
    }
}
