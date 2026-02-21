using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class ModelStateValidationExceptionHandlerTests
{
    private readonly ModelStateValidationExceptionHandler _handler;
    private readonly ErrorHandlingOptions _options;

    public ModelStateValidationExceptionHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var codeMapper = new ErrorCodeMapper(optionsWrapper);
        var msgMapper = new ErrorMessageMapper(optionsWrapper);
        _handler = new ModelStateValidationExceptionHandler(codeMapper, msgMapper, optionsWrapper);
    }

    [Fact]
    public void Order_Is90()
    {
        _handler.Order.Should().Be(90);
    }

    [Fact]
    public void CanHandle_ModelStateValidationException_ReturnsTrue()
    {
        var modelState = new ModelStateDictionary();
        _handler.CanHandle(new ModelStateValidationException(modelState)).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        _handler.CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Handle_WrongExceptionType_ThrowsInvalidOperationException()
    {
        var act = () => _handler.Handle(new InvalidOperationException());

        act.Should().Throw<InvalidOperationException>().WithMessage("*CanHandle*");
    }

    [Fact]
    public void Handle_RequiredError_InfersRequiredValidationType()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "The Email field is required.");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.Code.Should().Be("VALIDATION_FAILED");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Code.Should().Be("REQUIRED_NOT_NULL");
        response.FieldErrors[0].Property.Should().Be("email"); // camelCase
    }

    [Fact]
    public void Handle_StringLengthError_InfersStringLengthValidationType()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "The field Name must be a string with a minimum length of 2 and a maximum length of 100.");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Code.Should().Be("INVALID_SIZE");
    }

    [Fact]
    public void Handle_RangeError_InfersRangeValidationType()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Age", "Age must be between 18 and 120");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Code.Should().Be("VALUE_OUT_OF_RANGE");
    }

    [Fact]
    public void Handle_PatternError_InfersRegularExpressionType()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Code", "The field Code must match the regular expression pattern.");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Code.Should().Be("REGEX_PATTERN_VALIDATION_FAILED");
    }

    [Fact]
    public void Handle_JsonError_InfersJsonType()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("$", "The JSON value could not be converted.");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        // "$" field → global error
        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Code.Should().Be("JSON_PARSE_ERROR");
    }

    [Fact]
    public void Handle_OperatorPrecedenceRegression_JsonLiteralInvalid()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Value", "The value contains an invalid literal");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        // "invalid" AND "literal" → should infer Json type with fixed operator precedence
        response.FieldErrors![0].Code.Should().Be("JSON_PARSE_ERROR");
    }

    [Fact]
    public void Handle_DottedFieldName_ConvertsBothSegments()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Address.ZipCode", "ZipCode is required.");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Property.Should().Be("address.zipCode");
    }

    [Fact]
    public void Handle_MultipleFieldErrors_ReturnsAll()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "Name is required");
        modelState.AddModelError("Email", "Email is required");
        modelState.AddModelError("Age", "Age must be between 18 and 120");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(3);
    }

    [Fact]
    public void Handle_EmptyFieldName_CreatesGlobalError()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("", "Passwords do not match");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.GlobalErrors.Should().HaveCount(1);
        response.FieldErrors.Should().BeNull();
    }

    [Fact]
    public void Handle_WithAddPathToError_IncludesPath()
    {
        _options.AddPathToError = true;
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().Be("email");
    }

    [Fact]
    public void Handle_WithoutAddPathToError_ExcludesPath()
    {
        _options.AddPathToError = false;
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().BeNull();
    }

    [Fact]
    public void Handle_IncludeRejectedValuesTrue_IncludesRejectedValue()
    {
        _options.IncludeRejectedValues = true;
        var modelState = new ModelStateDictionary();
        modelState.SetModelValue("Email", "bad-email", "bad-email");
        modelState.AddModelError("Email", "Invalid email format");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].RejectedValue.Should().Be("bad-email");
    }

    [Fact]
    public void Handle_IncludeRejectedValuesFalse_ExcludesRejectedValue()
    {
        _options.IncludeRejectedValues = false;
        var modelState = new ModelStateDictionary();
        modelState.SetModelValue("Password", "secret123", "secret123");
        modelState.AddModelError("Password", "Password too short");

        var exception = new ModelStateValidationException(modelState);
        var response = _handler.Handle(exception);

        response.FieldErrors![0].RejectedValue.Should().BeNull();
    }
}
