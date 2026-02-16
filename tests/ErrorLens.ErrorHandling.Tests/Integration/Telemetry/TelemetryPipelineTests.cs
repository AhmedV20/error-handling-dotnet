using System.Diagnostics;
using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using ErrorLens.ErrorHandling.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.Telemetry;

[Collection("Telemetry")]
public class TelemetryPipelineTests : IDisposable
{
    private readonly List<Activity> _capturedActivities = new();
    private ActivityListener? _listener;

    private ErrorHandlingFacade CreateFacade(
        ErrorHandlingOptions? options = null,
        IEnumerable<IApiExceptionHandler>? handlers = null,
        IFallbackApiExceptionHandler? fallbackHandler = null)
    {
        var opts = options ?? new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(opts);

        var fallback = fallbackHandler ?? Substitute.For<IFallbackApiExceptionHandler>();
        fallback.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An error occurred"));

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var loggingService = Substitute.For<ILoggingService>();

        return new ErrorHandlingFacade(
            handlers ?? Enumerable.Empty<IApiExceptionHandler>(),
            fallback,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger,
            loggingService);
    }

    private void StartListening()
    {
        // Dispose any previous listener and clear captured activities for test isolation
        _listener?.Dispose();
        _capturedActivities.Clear();

        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ErrorHandlingActivitySource.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => _capturedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    /// <summary>
    /// Gets the activity produced by this test, filtering by exception type tag
    /// and exception message to isolate from parallel test classes that also call HandleException.
    /// </summary>
    private Activity GetTestActivity(
        string expectedExceptionType = "System.InvalidOperationException",
        string? expectedMessage = null)
    {
        return _capturedActivities.Last(a =>
            a.OperationName == "ErrorLens.HandleException" &&
            a.GetTagItem("error.type") as string == expectedExceptionType &&
            (expectedMessage == null || a.Events.Any(e =>
                e.Name == "exception" &&
                e.Tags.Any(t => t.Key == "exception.message" && t.Value as string == expectedMessage))));
    }

    // --- Tests WITH ActivityListener ---

    [Fact]
    public void HandleException_WithListener_CreatesActivityWithCorrectName()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_activity_name");

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_activity_name");
        activity.DisplayName.Should().Be("ErrorLens.HandleException");
    }

    [Fact]
    public void HandleException_WithListener_SetsErrorCodeTag()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_error_code");

        var response = facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_error_code");
        activity.GetTagItem("error.code").Should().Be(response.Code);
    }

    [Fact]
    public void HandleException_WithListener_SetsErrorTypeTag()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_error_type");

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_error_type");
        activity.GetTagItem("error.type").Should().Be("System.InvalidOperationException");
    }

    [Fact]
    public void HandleException_WithListener_SetsHttpStatusCodeTag()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_status_code");

        var response = facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_status_code");
        activity.GetTagItem("http.response.status_code").Should().Be((int)response.HttpStatusCode);
    }

    [Fact]
    public void HandleException_WithListener_SetsStatusToError()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_status_error");

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_status_error");
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HandleException_WithListener_AddsExceptionEvent()
    {
        StartListening();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("telemetry_test_exception_event");

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_exception_event");
        var exceptionEvent = activity.Events.FirstOrDefault(e => e.Name == "exception");
        exceptionEvent.Name.Should().Be("exception");

        var tags = exceptionEvent.Tags.ToDictionary(t => t.Key, t => t.Value);
        tags.Should().ContainKey("exception.type");
        tags["exception.type"].Should().Be("System.InvalidOperationException");
        tags.Should().ContainKey("exception.message");
        tags["exception.message"].Should().Be("telemetry_test_exception_event");
    }

    [Fact]
    public void HandleException_WithListener_AlwaysIncludesStacktrace()
    {
        StartListening();
        var facade = CreateFacade();

        // Create exception with a stacktrace by throwing and catching
        Exception exception;
        try
        {
            throw new InvalidOperationException("telemetry_test_stacktrace");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_stacktrace");
        var exceptionEvent = activity.Events.FirstOrDefault(e => e.Name == "exception");
        var tags = exceptionEvent.Tags.ToDictionary(t => t.Key, t => t.Value);

        tags.Should().ContainKey("exception.stacktrace");
        (tags["exception.stacktrace"] as string).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HandleException_WithListener_IncludesStacktraceEvenFor5xx()
    {
        StartListening();

        var fallback = Substitute.For<IFallbackApiExceptionHandler>();
        fallback.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "SERVER_ERROR", "Internal server error"));

        var facade = CreateFacade(fallbackHandler: fallback);

        Exception exception;
        try
        {
            throw new Exception("telemetry_test_5xx_stacktrace");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        facade.HandleException(exception);

        var activity = GetTestActivity("System.Exception", "telemetry_test_5xx_stacktrace");
        var exceptionEvent = activity.Events.FirstOrDefault(e => e.Name == "exception");
        var tags = exceptionEvent.Tags.ToDictionary(t => t.Key, t => t.Value);

        tags.Should().ContainKey("exception.stacktrace");
        (tags["exception.stacktrace"] as string).Should().NotBeNullOrEmpty();
    }

    // --- Tests WITHOUT ActivityListener ---

    [Fact]
    public void HandleException_WithoutListener_StartActivityReturnsNull()
    {
        // No listener registered — StartActivity should return null
        var activity = ErrorHandlingActivitySource.Source.StartActivity("test");
        activity.Should().BeNull();
    }

    [Fact]
    public void HandleException_WithoutListener_StillReturnsValidResponse()
    {
        // Without listener, facade should still work normally
        var facade = CreateFacade();
        var exception = new InvalidOperationException("test error");

        var response = facade.HandleException(exception);

        response.Should().NotBeNull();
        response.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HandleException_WithoutListener_ProducesNoActivities()
    {
        // Ensure no activities are captured when no listener
        _capturedActivities.Clear();
        var facade = CreateFacade();
        var exception = new InvalidOperationException("test error");

        facade.HandleException(exception);

        _capturedActivities.Should().BeEmpty();
    }

    // --- Tests with custom handler ---

    [Fact]
    public void HandleException_WithCustomHandler_TagsReflectHandlerResponse()
    {
        StartListening();

        var handler = Substitute.For<IApiExceptionHandler>();
        handler.Order.Returns(0);
        handler.CanHandle(Arg.Any<Exception>()).Returns(true);
        handler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.NotFound, "NOT_FOUND", "Resource not found"));

        var facade = CreateFacade(handlers: new[] { handler });
        var exception = new InvalidOperationException("telemetry_test_custom_handler");

        facade.HandleException(exception);

        var activity = GetTestActivity(expectedMessage: "telemetry_test_custom_handler");
        activity.GetTagItem("error.code").Should().Be("NOT_FOUND");
        activity.GetTagItem("http.response.status_code").Should().Be(404);
    }

    public void Dispose()
    {
        _listener?.Dispose();
        _capturedActivities.Clear();
    }
}
