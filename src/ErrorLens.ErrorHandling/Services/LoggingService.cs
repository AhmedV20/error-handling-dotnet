using System.Net;
using System.Text.RegularExpressions;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Service for logging exception handling events.
/// </summary>
public partial class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ErrorHandlingOptions _options;
    private readonly IEnumerable<ILoggingFilter> _filters;

    public LoggingService(
        ILogger<LoggingService> logger,
        IOptions<ErrorHandlingOptions> options,
        IEnumerable<ILoggingFilter> filters)
    {
        _logger = logger;
        _options = options.Value;
        _filters = filters;
    }

    /// <inheritdoc />
    public void LogException(Exception exception, ApiErrorResponse response)
    {
        // Check if any filter excludes this exception
        foreach (var filter in _filters)
        {
            if (!filter.ShouldLog(response, exception))
            {
                return;
            }
        }

        if (_options.ExceptionLogging == ExceptionLogging.None)
        {
            return;
        }

        var logLevel = GetLogLevel(response.HttpStatusCode);
        var includeStackTrace = ShouldIncludeStackTrace(response.HttpStatusCode, exception);

        if (includeStackTrace || _options.ExceptionLogging == ExceptionLogging.WithStacktrace)
        {
            _logger.Log(logLevel, exception, "Exception handled: {Code} - {Message}", response.Code, response.Message);
        }
        else
        {
            _logger.Log(logLevel, "Exception handled: {Code} - {Message}", response.Code, response.Message);
        }
    }

    private LogLevel GetLogLevel(HttpStatusCode statusCode)
    {
        var statusInt = (int)statusCode;
        var statusStr = statusInt.ToString();

        // Check exact match first
        if (_options.LogLevels.TryGetValue(statusStr, out var exactLevel))
        {
            return exactLevel;
        }

        // Check pattern match (e.g., "4xx", "5xx")
        var pattern = $"{statusStr[0]}xx";
        if (_options.LogLevels.TryGetValue(pattern, out var patternLevel))
        {
            return patternLevel;
        }

        // Default log levels based on status code range
        return statusInt switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    private bool ShouldIncludeStackTrace(HttpStatusCode statusCode, Exception exception)
    {
        var statusInt = (int)statusCode;
        var statusStr = statusInt.ToString();
        var pattern = $"{statusStr[0]}xx";

        // Check if status is in full stacktrace list
        if (_options.FullStacktraceHttpStatuses.Contains(statusStr) ||
            _options.FullStacktraceHttpStatuses.Contains(pattern))
        {
            return true;
        }

        // Check if exception class is in full stacktrace list
        var exceptionType = exception.GetType();
        var typeName = exceptionType.FullName ?? exceptionType.Name;
        if (_options.FullStacktraceClasses.Contains(typeName))
        {
            return true;
        }

        return false;
    }
}
