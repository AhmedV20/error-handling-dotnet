using System.Diagnostics;
using System.Runtime.ExceptionServices;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Central facade for exception handling orchestration.
/// Coordinates handlers, customizers, logging, and localization.
/// </summary>
public class ErrorHandlingFacade
{
    private readonly IEnumerable<IApiExceptionHandler> _handlers;
    private readonly IFallbackApiExceptionHandler _fallbackHandler;
    private readonly IEnumerable<IApiErrorResponseCustomizer> _customizers;
    private readonly ILoggingService? _loggingService;
    private readonly IErrorMessageLocalizer? _localizer;
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<ErrorHandlingFacade> _logger;

    public ErrorHandlingFacade(
        IEnumerable<IApiExceptionHandler> handlers,
        IFallbackApiExceptionHandler fallbackHandler,
        IEnumerable<IApiErrorResponseCustomizer> customizers,
        IOptions<ErrorHandlingOptions> options,
        ILogger<ErrorHandlingFacade> logger,
        ILoggingService? loggingService = null,
        IErrorMessageLocalizer? localizer = null)
    {
        // Sort handlers by order (lower = higher priority)
        _handlers = handlers.OrderBy(h => h.Order).ToList();
        _fallbackHandler = fallbackHandler;
        _customizers = customizers.ToList();
        _loggingService = loggingService;
        _localizer = localizer;
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
            // If disabled, rethrow preserving original stack trace
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        using var activity = ErrorHandlingActivitySource.Source.StartActivity("ErrorLens.HandleException");

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

            // Localize error messages (no-op when NoOpErrorMessageLocalizer is registered)
            if (_localizer != null)
            {
                response.Message = _localizer.Localize(response.Code, response.Message);

                if (response.FieldErrors != null)
                {
                    foreach (var fieldError in response.FieldErrors)
                    {
                        fieldError.Message = _localizer.LocalizeFieldError(fieldError.Code, fieldError.Property, fieldError.Message)!;
                    }
                }

                if (response.GlobalErrors != null)
                {
                    foreach (var globalError in response.GlobalErrors)
                    {
                        globalError.Message = _localizer.Localize(globalError.Code, globalError.Message)!;
                    }
                }

                if (response.ParameterErrors != null)
                {
                    foreach (var parameterError in response.ParameterErrors)
                    {
                        parameterError.Message = _localizer.Localize(parameterError.Code, parameterError.Message)!;
                    }
                }
            }

            // Enrich activity with telemetry data (zero overhead when no collector configured)
            if (activity?.IsAllDataRequested == true)
            {
                activity.SetTag("error.code", response.Code);
                activity.SetTag("error.type", exception.GetType().FullName);
                activity.SetTag("http.response.status_code", (int)response.HttpStatusCode);

                // Add exception event with OTel semantic conventions
                var exceptionTags = new ActivityTagsCollection
                {
                    { "exception.type", exception.GetType().FullName },
                    { "exception.message", exception.Message },
                    { "exception.stacktrace", exception.StackTrace ?? exception.ToString() }
                };
                activity.AddEvent(new ActivityEvent("exception", tags: exceptionTags));

                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            }
        }
        catch (Exception handlerException)
        {
            _logger.LogError(handlerException, "Error in exception handler, falling back to default response");
            _logger.LogError(exception, "Original exception that triggered the failed handler");

            // If handler throws, return a safe default response
            response = new ApiErrorResponse(
                System.Net.HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                _options.FallbackMessage);
        }

        return response;
    }
}
