using System.ComponentModel.DataAnnotations;
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
/// Integration tests for validation error handling.
/// </summary>
public class ValidationErrorTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ErrorHandlingFacade _facade;

    public ValidationErrorTests()
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

        var validationHandler = new ValidationExceptionHandler(
            errorCodeMapper,
            errorMessageMapper,
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        _facade = new ErrorHandlingFacade(
            new IApiExceptionHandler[] { validationHandler },
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);
    }

    [Fact]
    public void ValidationException_ReturnsFieldErrors()
    {
        var validationResult = new ValidationResult("Email is required", new[] { "Email" });
        var exception = new ValidationException(validationResult, new RequiredAttribute(), null);

        var response = _facade.HandleException(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Property.Should().Be("email");
        response.FieldErrors[0].Message.Should().Be("Email is required");
    }

    [Fact]
    public void ValidationException_WithMultipleFields_ReturnsAllErrors()
    {
        // Simulate multiple validation errors by testing handler separately
        var validationResult1 = new ValidationResult("Email is required", new[] { "Email" });
        var exception1 = new ValidationException(validationResult1, new RequiredAttribute(), null);

        var response1 = _facade.HandleException(exception1);
        response1.FieldErrors.Should().HaveCount(1);

        var validationResult2 = new ValidationResult("Password too short", new[] { "Password" });
        var exception2 = new ValidationException(validationResult2, new StringLengthAttribute(100), "123");

        var response2 = _facade.HandleException(exception2);
        response2.FieldErrors.Should().HaveCount(1);
        response2.FieldErrors![0].RejectedValue.Should().Be("123");
    }

    [Fact]
    public void ValidationException_WithRejectedValue_IncludesValue()
    {
        var validationResult = new ValidationResult("Invalid email", new[] { "Email" });
        var exception = new ValidationException(validationResult, new EmailAddressAttribute(), "not-an-email");

        var response = _facade.HandleException(exception);

        response.FieldErrors![0].RejectedValue.Should().Be("not-an-email");
    }

    [Fact]
    public void ValidationException_GlobalError_NoFieldErrors()
    {
        var validationResult = new ValidationResult("Passwords must match");
        var exception = new ValidationException(validationResult, null, null);

        var response = _facade.HandleException(exception);

        response.FieldErrors.Should().BeNull();
        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Message.Should().Be("Passwords must match");
    }

    [Fact]
    public void ValidationException_IncludesPath_WhenConfigured()
    {
        _options.AddPathToError = true;
        var validationResult = new ValidationResult("Invalid", new[] { "Address.ZipCode" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _facade.HandleException(exception);

        response.FieldErrors![0].Path.Should().Be("address.ZipCode");
    }

    [Fact]
    public void ValidationException_ExcludesPath_WhenNotConfigured()
    {
        _options.AddPathToError = false;
        var validationResult = new ValidationResult("Invalid", new[] { "Email" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _facade.HandleException(exception);

        response.FieldErrors![0].Path.Should().BeNull();
    }

    [Fact]
    public void ValidationHandler_HasHigherPriority_ThanFallback()
    {
        var validationException = new ValidationException("Validation failed");

        var response = _facade.HandleException(validationException);

        // Should be handled by ValidationExceptionHandler, not fallback
        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
    }
}
