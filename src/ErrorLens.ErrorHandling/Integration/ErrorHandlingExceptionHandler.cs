#if NET8_0_OR_GREATER
using ErrorLens.ErrorHandling.Configuration;
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
    private readonly ErrorResponseWriter _responseWriter;

    public ErrorHandlingExceptionHandler(
        ErrorHandlingFacade facade,
        IOptions<ErrorHandlingOptions> options,
        ErrorResponseWriter responseWriter)
    {
        _facade = facade;
        _options = options.Value;
        _responseWriter = responseWriter;
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
        await _responseWriter.WriteResponseAsync(httpContext, response, cancellationToken);

        return true;
    }
}
#endif
