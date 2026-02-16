using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.Localization;

public class LocalizationPipelineTests
{
    private ErrorHandlingFacade CreateFacade(
        IErrorMessageLocalizer? localizer = null,
        IEnumerable<IApiExceptionHandler>? handlers = null,
        IFallbackApiExceptionHandler? fallbackHandler = null)
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

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
            loggingService,
            localizer);
    }

    [Fact]
    public void HandleException_WithLocalizer_LocalizesTopLevelMessage()
    {
        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize("INTERNAL_ERROR", "An error occurred")
            .Returns("Ein Fehler ist aufgetreten");

        var facade = CreateFacade(localizer: localizer);
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.Message.Should().Be("Ein Fehler ist aufgetreten");
    }

    [Fact]
    public void HandleException_WithLocalizer_LocalizesFieldErrorMessages()
    {
        var handler = Substitute.For<IApiExceptionHandler>();
        handler.Order.Returns(0);
        handler.CanHandle(Arg.Any<Exception>()).Returns(true);
        handler.Handle(Arg.Any<Exception>()).Returns(_ =>
        {
            var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
            response.AddFieldError(new ApiFieldError("REQUIRED_NOT_NULL", "email", "Must not be null"));
            response.AddFieldError(new ApiFieldError("INVALID_FORMAT", "phone", "Invalid format"));
            return response;
        });

        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize("VALIDATION_FAILED", "Validation failed").Returns("Validierung fehlgeschlagen");
        localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "email", "Must not be null").Returns("Darf nicht leer sein");
        localizer.LocalizeFieldError("INVALID_FORMAT", "phone", "Invalid format").Returns("Ungültiges Format");

        var facade = CreateFacade(localizer: localizer, handlers: new[] { handler });
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.Message.Should().Be("Validierung fehlgeschlagen");
        response.FieldErrors.Should().HaveCount(2);
        response.FieldErrors![0].Message.Should().Be("Darf nicht leer sein");
        response.FieldErrors![1].Message.Should().Be("Ungültiges Format");
    }

    [Fact]
    public void HandleException_WithLocalizer_LocalizesGlobalErrorMessages()
    {
        var handler = Substitute.For<IApiExceptionHandler>();
        handler.Order.Returns(0);
        handler.CanHandle(Arg.Any<Exception>()).Returns(true);
        handler.Handle(Arg.Any<Exception>()).Returns(_ =>
        {
            var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
            response.AddGlobalError(new ApiGlobalError("CROSS_FIELD_INVALID", "Start date must be before end date"));
            return response;
        });

        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize(Arg.Any<string>(), Arg.Any<string?>()).Returns(ci => ci.ArgAt<string?>(1));
        localizer.Localize("CROSS_FIELD_INVALID", "Start date must be before end date")
            .Returns("Startdatum muss vor Enddatum liegen");

        var facade = CreateFacade(localizer: localizer, handlers: new[] { handler });
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.GlobalErrors.Should().ContainSingle();
        response.GlobalErrors![0].Message.Should().Be("Startdatum muss vor Enddatum liegen");
    }

    [Fact]
    public void HandleException_WithNoOpLocalizer_ProducesUnchangedMessages()
    {
        var localizer = new NoOpErrorMessageLocalizer();
        var facade = CreateFacade(localizer: localizer);
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.Message.Should().Be("An error occurred");
    }

    [Fact]
    public void HandleException_WithNullLocalizer_ProducesUnchangedMessages()
    {
        var facade = CreateFacade(localizer: null);
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.Message.Should().Be("An error occurred");
    }

    [Fact]
    public void HandleException_CustomLocalizerRegisteredFirst_TakesPrecedence()
    {
        var customLocalizer = Substitute.For<IErrorMessageLocalizer>();
        customLocalizer.Localize(Arg.Any<string>(), Arg.Any<string?>())
            .Returns("Custom localized message");

        var facade = CreateFacade(localizer: customLocalizer);
        var exception = new InvalidOperationException("test");

        var response = facade.HandleException(exception);

        response.Message.Should().Be("Custom localized message");
    }
}
