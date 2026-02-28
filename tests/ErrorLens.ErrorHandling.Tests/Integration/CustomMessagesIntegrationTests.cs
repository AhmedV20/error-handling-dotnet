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
/// Integration tests for configurable fallback message (US2) and built-in handler messages (US3).
/// </summary>
public class CustomMessagesIntegrationTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ErrorHandlingFacade _facade;

    public CustomMessagesIntegrationTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper,
            errorMessageMapper,
            httpStatusMapper,
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        _facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);
    }

    [Fact]
    public void Pipeline_CustomFallbackMessage_AppearsIn5xxResponse()
    {
        _options.FallbackMessage = "Something went wrong. Please contact support at help@example.com";
        var exception = new Exception("Database connection failed: server=prod;password=secret");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("Something went wrong. Please contact support at help@example.com");
    }

    [Fact]
    public void Pipeline_DefaultFallbackMessage_UsedWhenNotConfigured()
    {
        var exception = new Exception("Unhandled error");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void Pipeline_4xxException_NotAffectedByCustomFallbackMessage()
    {
        _options.FallbackMessage = "Custom 5xx message";
        var exception = new ArgumentException("The 'name' parameter is required");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Message.Should().Be("The 'name' parameter is required");
    }

    [Fact]
    public void Pipeline_HandlerException_CatchBlockUsesCustomFallbackMessage()
    {
        _options.FallbackMessage = "Service temporarily unavailable";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper,
            errorMessageMapper,
            httpStatusMapper,
            optionsWrapper);

        // Create a handler that throws when processing
        var brokenHandler = Substitute.For<IApiExceptionHandler>();
        brokenHandler.CanHandle(Arg.Any<Exception>()).Returns(true);
        brokenHandler.Handle(Arg.Any<Exception>()).Returns(_ => throw new InvalidOperationException("Handler crashed"));

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        var facade = new ErrorHandlingFacade(
            new[] { brokenHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        var exception = new Exception("Original error");

        var response = facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Code.Should().Be("INTERNAL_SERVER_ERROR");
        response.Message.Should().Be("Service temporarily unavailable");
    }

    // US3: BuiltInMessages integration tests

    [Fact]
    public void Pipeline_JsonException_UsesCustomBuiltInMessage()
    {
        _options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "Invalid JSON in request body";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var jsonHandler = new JsonExceptionHandler(optionsWrapper);
        var fallbackHandler = new DefaultFallbackHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            new HttpStatusMapper(optionsWrapper),
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { jsonHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        var response = facade.HandleException(new System.Text.Json.JsonException("parse error"));

        response.Code.Should().Be("MESSAGE_NOT_READABLE");
        response.Message.Should().Be("Invalid JSON in request body");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Pipeline_TypeMismatch_UsesCustomBuiltInMessage()
    {
        _options.BuiltInMessages["TYPE_MISMATCH"] = "Value type does not match expected type";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var typeMismatchHandler = new TypeMismatchExceptionHandler(optionsWrapper);
        var fallbackHandler = new DefaultFallbackHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            new HttpStatusMapper(optionsWrapper),
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { typeMismatchHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        var response = facade.HandleException(new FormatException("bad format"));

        response.Code.Should().Be("TYPE_MISMATCH");
        response.Message.Should().Be("Value type does not match expected type");
    }

    [Fact]
    public void Pipeline_BadRequest_UsesCustomBuiltInMessage()
    {
        _options.BuiltInMessages["BAD_REQUEST"] = "The request is invalid";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var badRequestHandler = new BadRequestExceptionHandler(optionsWrapper);
        var fallbackHandler = new DefaultFallbackHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            new HttpStatusMapper(optionsWrapper),
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { badRequestHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        var response = facade.HandleException(
            new Microsoft.AspNetCore.Http.BadHttpRequestException("System.Text.Json deserialization failed"));

        response.Code.Should().Be("BAD_REQUEST");
        response.Message.Should().Be("The request is invalid");
    }

    [Fact]
    public void Pipeline_ValidationFailed_UsesCustomBuiltInMessage()
    {
        _options.BuiltInMessages["VALIDATION_FAILED"] = "Input validation error";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var validationHandler = new ValidationExceptionHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            optionsWrapper);
        var fallbackHandler = new DefaultFallbackHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            new HttpStatusMapper(optionsWrapper),
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { validationHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        var response = facade.HandleException(
            new System.ComponentModel.DataAnnotations.ValidationException("Original message"));

        response.Code.Should().Be("VALIDATION_FAILED");
        response.Message.Should().Be("Input validation error");
    }

    [Fact]
    public void Pipeline_PartialBuiltInMessages_DefaultsForUnconfiguredKeys()
    {
        // Only configure MESSAGE_NOT_READABLE, leave others at default
        _options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "Custom JSON error";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var jsonHandler = new JsonExceptionHandler(optionsWrapper);
        var typeMismatchHandler = new TypeMismatchExceptionHandler(optionsWrapper);
        var fallbackHandler = new DefaultFallbackHandler(
            new ErrorCodeMapper(optionsWrapper),
            new ErrorMessageMapper(optionsWrapper),
            new HttpStatusMapper(optionsWrapper),
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { jsonHandler, typeMismatchHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        // Configured key uses custom message
        var jsonResponse = facade.HandleException(new System.Text.Json.JsonException("parse error"));
        jsonResponse.Message.Should().Be("Custom JSON error");

        // Unconfigured key uses default message
        var typeMismatchResponse = facade.HandleException(new FormatException("bad format"));
        typeMismatchResponse.Message.Should().Be("A type conversion error occurred");
    }
}
