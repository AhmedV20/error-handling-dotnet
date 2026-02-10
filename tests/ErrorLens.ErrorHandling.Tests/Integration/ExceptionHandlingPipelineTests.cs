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
/// Integration tests for the complete exception handling pipeline.
/// </summary>
public class ExceptionHandlingPipelineTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ErrorHandlingFacade _facade;

    public ExceptionHandlingPipelineTests()
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
    public void Pipeline_InvalidOperationException_ReturnsStructuredResponse()
    {
        var exception = new InvalidOperationException("Something went wrong");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("INVALID_OPERATION");
        response.Message.Should().Be("Something went wrong");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Pipeline_ArgumentException_ReturnsBadRequest()
    {
        var exception = new ArgumentException("Invalid argument");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("ARGUMENT");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Pipeline_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var exception = new UnauthorizedAccessException("Access denied");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("UNAUTHORIZED_ACCESS");
        response.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Pipeline_KeyNotFoundException_ReturnsNotFound()
    {
        var exception = new KeyNotFoundException("User not found");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("KEY_NOT_FOUND");
        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Pipeline_GenericException_ReturnsInternalServerError()
    {
        var exception = new Exception("Unknown error");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Pipeline_WithConfiguredOverride_UsesConfiguredCode()
    {
        _options.Codes["System.InvalidOperationException"] = "CUSTOM_ERROR";
        var exception = new InvalidOperationException("Test");

        var response = _facade.HandleException(exception);

        response.Code.Should().Be("CUSTOM_ERROR");
    }

    [Fact]
    public void Pipeline_WithConfiguredStatus_UsesConfiguredStatus()
    {
        _options.HttpStatuses["System.InvalidOperationException"] = HttpStatusCode.ServiceUnavailable;
        var exception = new InvalidOperationException("Test");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public void Pipeline_WithHttpStatusInJson_IncludesStatus()
    {
        _options.HttpStatusInJsonResponse = true;
        var exception = new InvalidOperationException("Test");

        var response = _facade.HandleException(exception);

        response.Status.Should().Be(400);
    }

    [Fact]
    public void Pipeline_WithCustomizer_AppliesCustomization()
    {
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

        var customizer = Substitute.For<IApiErrorResponseCustomizer>();
        customizer.When(c => c.Customize(Arg.Any<ApiErrorResponse>()))
            .Do(c => c.Arg<ApiErrorResponse>().AddProperty("traceId", "abc123"));

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            new[] { customizer },
            optionsWrapper,
            logger);

        var response = facade.HandleException(new Exception("Test"));

        response.Properties.Should().ContainKey("traceId");
        response.Properties!["traceId"].Should().Be("abc123");
    }
}
