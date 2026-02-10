using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Services;

public class ErrorHandlingFacadeTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly IFallbackApiExceptionHandler _fallbackHandler;
    private readonly ILogger<ErrorHandlingFacade> _logger;
    private readonly ErrorHandlingFacade _facade;

    public ErrorHandlingFacadeTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        _fallbackHandler = Substitute.For<IFallbackApiExceptionHandler>();
        _fallbackHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "An error occurred"));

        _logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        _facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            _fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            _logger);
    }

    [Fact]
    public void HandleException_WithNoHandlers_UsesFallback()
    {
        var exception = new Exception("test");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("ERROR");
        _fallbackHandler.Received(1).Handle(exception);
    }

    [Fact]
    public void HandleException_WithMatchingHandler_UsesHandler()
    {
        var customHandler = Substitute.For<IApiExceptionHandler>();
        customHandler.CanHandle(Arg.Any<Exception>()).Returns(true);
        customHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.BadRequest, "CUSTOM", "Custom error"));
        customHandler.Order.Returns(100);

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var facade = new ErrorHandlingFacade(
            new[] { customHandler },
            _fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            _logger);

        var response = facade.HandleException(new Exception("test"));

        response.Code.Should().Be("CUSTOM");
        customHandler.Received(1).Handle(Arg.Any<Exception>());
        _fallbackHandler.DidNotReceive().Handle(Arg.Any<Exception>());
    }

    [Fact]
    public void HandleException_AppliesCustomizers()
    {
        var customizer = Substitute.For<IApiErrorResponseCustomizer>();
        customizer.When(c => c.Customize(Arg.Any<ApiErrorResponse>()))
            .Do(c => c.Arg<ApiErrorResponse>().AddProperty("customized", true));

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            _fallbackHandler,
            new[] { customizer },
            optionsWrapper,
            _logger);

        var response = facade.HandleException(new Exception("test"));

        response.Properties.Should().ContainKey("customized");
        customizer.Received(1).Customize(Arg.Any<ApiErrorResponse>());
    }

    [Fact]
    public void HandleException_WhenDisabled_ThrowsException()
    {
        _options.Enabled = false;
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            _fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            _logger);

        var exception = new Exception("test");

        var act = () => facade.HandleException(exception);

        act.Should().Throw<Exception>().WithMessage("test");
    }

    [Fact]
    public void HandleException_IncludesStatusInJson_WhenConfigured()
    {
        _options.HttpStatusInJsonResponse = true;
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            _fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            _logger);

        var response = facade.HandleException(new Exception("test"));

        response.Status.Should().Be(500);
    }

    [Fact]
    public void HandleException_HandlersOrderedByPriority()
    {
        var lowPriorityHandler = Substitute.For<IApiExceptionHandler>();
        lowPriorityHandler.CanHandle(Arg.Any<Exception>()).Returns(true);
        lowPriorityHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse("LOW_PRIORITY"));
        lowPriorityHandler.Order.Returns(200);

        var highPriorityHandler = Substitute.For<IApiExceptionHandler>();
        highPriorityHandler.CanHandle(Arg.Any<Exception>()).Returns(true);
        highPriorityHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse("HIGH_PRIORITY"));
        highPriorityHandler.Order.Returns(100);

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        // Add in reverse order to verify sorting
        var facade = new ErrorHandlingFacade(
            new[] { lowPriorityHandler, highPriorityHandler },
            _fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            _logger);

        var response = facade.HandleException(new Exception("test"));

        response.Code.Should().Be("HIGH_PRIORITY");
    }
}
