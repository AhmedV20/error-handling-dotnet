using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Central facade for exception handling orchestration.
/// Coordinates handlers, customizers, and logging.
/// </summary>
public class ErrorHandlingFacade
{
    private readonly IEnumerable<IApiExceptionHandler> _handlers;
    private readonly IFallbackApiExceptionHandler _fallbackHandler;
    private readonly IEnumerable<IApiErrorResponseCustomizer> _customizers;
    private readonly ILoggingService? _loggingService;
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<ErrorHandlingFacade> _logger;

    public ErrorHandlingFacade(
        IEnumerable<IApiExceptionHandler> handlers,
        IFallbackApiExceptionHandler fallbackHandler,
        IEnumerable<IApiErrorResponseCustomizer> customizers,
        IOptions<ErrorHandlingOptions> options,
        ILogger<ErrorHandlingFacade> logger,
        ILoggingService? loggingService = null)
    {
        // Sort handlers by order (lower = higher priority)
        _handlers = handlers.OrderBy(h => h.Order).ToList();
        _fallbackHandler = fallbackHandler;
        _customizers = customizers;
        _loggingService = loggingService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles an exception and produces an error response.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>The error response.</returns>
    public ApiErrorResponse HandleException(Exception exception)
    {
        if (!_options.Enabled)
        {
            // If disabled, rethrow
            throw exception;
        }

        ApiErrorResponse response;

        try
        {
            // Find first handler that can handle this exception
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(exception));

            response = handler != null
                ? handler.Handle(exception)
                : _fallbackHandler.Handle(exception);

            // Include HTTP status in JSON if configured
            if (_options.HttpStatusInJsonResponse && response.Status == 0)
            {
                response.Status = (int)response.HttpStatusCode;
            }

            // Apply all customizers
            foreach (var customizer in _customizers)
            {
                customizer.Customize(response);
            }

            // Log the exception
            _loggingService?.LogException(exception, response);
        }
        catch (Exception handlerException)
        {
            _logger.LogError(handlerException, "Error in exception handler, falling back to default response");

            // If handler throws, return a safe default response
            response = new ApiErrorResponse(
                System.Net.HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred");
        }

        return response;
    }
}
