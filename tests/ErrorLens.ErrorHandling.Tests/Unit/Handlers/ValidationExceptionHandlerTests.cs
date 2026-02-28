using System.ComponentModel.DataAnnotations;
using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class ValidationExceptionHandlerTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly ValidationExceptionHandler _handler;

    public ValidationExceptionHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        _errorCodeMapper = Substitute.For<IErrorCodeMapper>();
        _errorCodeMapper.GetErrorCode(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(1)); // Return default code

        _errorMessageMapper = Substitute.For<IErrorMessageMapper>();
        _errorMessageMapper.GetErrorMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(2)); // Return default message

        _handler = new ValidationExceptionHandler(_errorCodeMapper, _errorMessageMapper, optionsWrapper);
    }

    [Fact]
    public void CanHandle_ValidationException_ReturnsTrue()
    {
        var exception = new ValidationException("Validation failed");

        var result = _handler.CanHandle(exception);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        var exception = new Exception("Other error");

        var result = _handler.CanHandle(exception);

        result.Should().BeFalse();
    }

    [Fact]
    public void Handle_ReturnsValidationFailedCode()
    {
        var exception = new ValidationException("Validation failed");

        var response = _handler.Handle(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Handle_WithValidationResult_CreatesFieldError()
    {
        var validationResult = new ValidationResult("Email is required", new[] { "Email" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Property.Should().Be("email");
        response.FieldErrors[0].Message.Should().Be("Email is required");
    }

    [Fact]
    public void Handle_WithRequiredAttribute_UsesRequiredCode()
    {
        var attribute = new RequiredAttribute();
        var validationResult = new ValidationResult("The field is required", new[] { "Name" });
        var exception = new ValidationException(validationResult, attribute, null);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(1);
        _errorCodeMapper.Received(1).GetErrorCode(
            Arg.Is<string>(s => s.Contains("Name")),
            DefaultErrorCodes.RequiredNotNull);
    }

    [Fact]
    public void Handle_WithRangeAttribute_UsesRangeCode()
    {
        var attribute = new RangeAttribute(1, 100);
        var validationResult = new ValidationResult("Value out of range", new[] { "Age" });
        var exception = new ValidationException(validationResult, attribute, 150);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].RejectedValue.Should().Be(150);
    }

    [Fact]
    public void Handle_WithNoMemberNames_CreatesGlobalError()
    {
        var validationResult = new ValidationResult("Global validation failed");
        var exception = new ValidationException(validationResult, null, null);

        var response = _handler.Handle(exception);

        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Message.Should().Be("Global validation failed");
    }

    [Fact]
    public void Handle_IncludesPath_WhenConfigured()
    {
        _options.AddPathToError = true;
        var validationResult = new ValidationResult("Invalid", new[] { "Email" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().Be("email");
    }

    [Fact]
    public void Handle_ExcludesPath_WhenNotConfigured()
    {
        _options.AddPathToError = false;
        var validationResult = new ValidationResult("Invalid", new[] { "Email" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().BeNull();
    }

    [Fact]
    public void Order_Returns100()
    {
        _handler.Order.Should().Be(100);
    }

    // US3: BuiltInMessages tests

    [Fact]
    public void Handle_CustomBuiltInMessage_UsedForTopLevelMessage()
    {
        _options.BuiltInMessages["VALIDATION_FAILED"] = "Input validation error";
        var exception = new ValidationException("Original message");

        var response = _handler.Handle(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.Message.Should().Be("Input validation error");
    }

    [Fact]
    public void Handle_DefaultMessage_WhenKeyNotInBuiltInMessages()
    {
        var exception = new ValidationException("Validation failed");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Validation failed");
    }

    [Fact]
    public void Handle_EmptyStringBuiltInMessage_UsesEmptyString()
    {
        _options.BuiltInMessages["VALIDATION_FAILED"] = "";
        var exception = new ValidationException("Validation failed");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("");
    }

    [Fact]
    public void Handle_CustomBuiltInMessage_DoesNotAffectFieldErrors()
    {
        _options.BuiltInMessages["VALIDATION_FAILED"] = "Custom top-level";
        var validationResult = new ValidationResult("Email is required", new[] { "Email" });
        var exception = new ValidationException(validationResult, null, null);

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Custom top-level");
        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Message.Should().Be("Email is required");
    }
}
