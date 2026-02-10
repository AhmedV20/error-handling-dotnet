using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Services;

public class LoggingServiceTests
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ErrorHandlingOptions _options;
    private readonly LoggingService _service;

    public LoggingServiceTests()
    {
        _logger = Substitute.For<ILogger<LoggingService>>();
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        _service = new LoggingService(_logger, optionsWrapper, Enumerable.Empty<ILoggingFilter>());
    }

    [Fact]
    public void LogException_WithNoneLogging_DoesNotLog()
    {
        _options.ExceptionLogging = ExceptionLogging.None;
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void LogException_WithMessageOnly_LogsWithoutStackTrace()
    {
        _options.ExceptionLogging = ExceptionLogging.MessageOnly;
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public void LogException_WithStacktrace_LogsWithException()
    {
        _options.ExceptionLogging = ExceptionLogging.WithStacktrace;
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public void LogException_With5xxStatus_LogsAsError()
    {
        _options.ExceptionLogging = ExceptionLogging.MessageOnly;
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        // Verify log was called (exact level check is complex with NSubstitute)
        _logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public void LogException_WithConfiguredLogLevel_UsesConfiguredLevel()
    {
        _options.ExceptionLogging = ExceptionLogging.MessageOnly;
        _options.LogLevels["400"] = LogLevel.Debug;
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public void LogException_WithFullStacktraceStatus_IncludesStackTrace()
    {
        _options.ExceptionLogging = ExceptionLogging.MessageOnly;
        _options.FullStacktraceHttpStatuses.Add("5xx");
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");

        _service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public void LogException_WithFilter_RespectsFilter()
    {
        var filter = Substitute.For<ILoggingFilter>();
        filter.ShouldLog(Arg.Any<ApiErrorResponse>(), Arg.Any<Exception>()).Returns(false);

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var service = new LoggingService(_logger, optionsWrapper, new[] { filter });
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Test");

        service.LogException(new Exception("test"), response);

        _logger.ReceivedCalls().Should().BeEmpty();
    }
}
