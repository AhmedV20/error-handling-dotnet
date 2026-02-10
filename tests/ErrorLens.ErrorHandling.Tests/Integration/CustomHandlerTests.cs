using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration;

/// <summary>
/// Integration tests for custom handler registration.
/// </summary>
public class CustomHandlerTests
{
    [Fact]
    public void CustomHandler_IsUsed_WhenCanHandleReturnsTrue()
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var customHandler = new SqlExceptionHandler();
        var facade = CreateFacade(options, new[] { customHandler });

        var response = facade.HandleException(new FakeSqlException("Database error"));

        response.Code.Should().Be("DATABASE_ERROR");
        response.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public void CustomHandler_WithHigherPriority_WinsOverBuiltIn()
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var highPriorityHandler = new HighPriorityHandler();
        var lowPriorityHandler = new LowPriorityHandler();

        var facade = CreateFacade(options, new IApiExceptionHandler[] { lowPriorityHandler, highPriorityHandler });

        var response = facade.HandleException(new Exception("test"));

        response.Code.Should().Be("HIGH_PRIORITY");
    }

    [Fact]
    public void FallbackHandler_IsUsed_WhenNoHandlerMatches()
    {
        var options = new ErrorHandlingOptions();
        var facade = CreateFacade(options, Enumerable.Empty<IApiExceptionHandler>());

        var response = facade.HandleException(new Exception("test"));

        // Fallback uses the error code mapper
        response.Code.Should().NotBeNullOrEmpty();
    }

    private static ErrorHandlingFacade CreateFacade(
        ErrorHandlingOptions options,
        IEnumerable<IApiExceptionHandler> handlers)
    {
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper,
            errorMessageMapper,
            httpStatusMapper,
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        return new ErrorHandlingFacade(
            handlers,
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);
    }

    // Test exception types
    private class FakeSqlException : Exception
    {
        public FakeSqlException(string message) : base(message) { }
    }

    // Custom handlers for testing
    private class SqlExceptionHandler : AbstractApiExceptionHandler
    {
        public override int Order => 50;
        public override bool CanHandle(Exception exception) => exception is FakeSqlException;
        public override ApiErrorResponse Handle(Exception exception)
            => new(HttpStatusCode.ServiceUnavailable, "DATABASE_ERROR", exception.Message);
    }

    private class HighPriorityHandler : AbstractApiExceptionHandler
    {
        public override int Order => 10;
        public override bool CanHandle(Exception exception) => true;
        public override ApiErrorResponse Handle(Exception exception)
            => new("HIGH_PRIORITY");
    }

    private class LowPriorityHandler : AbstractApiExceptionHandler
    {
        public override int Order => 100;
        public override bool CanHandle(Exception exception) => true;
        public override ApiErrorResponse Handle(Exception exception)
            => new("LOW_PRIORITY");
    }
}
