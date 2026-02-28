using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Unit;

public class FluentValidationExceptionHandlerTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly FluentValidationOptions _fluentOptions;
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly IErrorMessageMapper _errorMessageMapper;
    private readonly FluentValidationExceptionHandler _handler;

    public FluentValidationExceptionHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        _fluentOptions = new FluentValidationOptions();
        var fluentOptionsWrapper = Substitute.For<IOptions<FluentValidationOptions>>();
        fluentOptionsWrapper.Value.Returns(_fluentOptions);

        _errorCodeMapper = Substitute.For<IErrorCodeMapper>();
        _errorCodeMapper.GetErrorCode(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(1));

        _errorMessageMapper = Substitute.For<IErrorMessageMapper>();
        _errorMessageMapper.GetErrorMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(2));

        _handler = new FluentValidationExceptionHandler(
            _errorCodeMapper, _errorMessageMapper, optionsWrapper, fluentOptionsWrapper);
    }

    // --- CanHandle tests ---

    [Fact]
    public void CanHandle_FluentValidationException_ReturnsTrue()
    {
        var exception = new ValidationException("Validation failed");
        _handler.CanHandle(exception).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        var exception = new InvalidOperationException("Other error");
        _handler.CanHandle(exception).Should().BeFalse();
    }

    [Fact]
    public void CanHandle_SystemValidationException_ReturnsFalse()
    {
        var exception = new System.ComponentModel.DataAnnotations.ValidationException("DataAnnotations");
        _handler.CanHandle(exception).Should().BeFalse();
    }

    // --- Order test ---

    [Fact]
    public void Order_Returns110()
    {
        _handler.Order.Should().Be(110);
    }

    // --- Handle: field-level errors ---

    [Fact]
    public void Handle_FieldLevelFailure_ProducesApiFieldError()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.") { ErrorCode = "NotEmptyValidator", AttemptedValue = "" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Code.Should().Be(DefaultErrorCodes.RequiredNotEmpty);
        response.FieldErrors[0].Property.Should().Be("email");
        response.FieldErrors[0].Message.Should().Be("'Email' must not be empty.");
        response.FieldErrors[0].RejectedValue.Should().Be("");
        response.FieldErrors[0].Path.Should().Be("email");
    }

    [Fact]
    public void Handle_MultipleFieldFailures_ProducesMultipleFieldErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.") { ErrorCode = "NotEmptyValidator", AttemptedValue = "" },
            new("Age", "'Age' must be greater than '0'.") { ErrorCode = "GreaterThanValidator", AttemptedValue = -5 }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(2);
        response.FieldErrors![0].Code.Should().Be(DefaultErrorCodes.RequiredNotEmpty);
        response.FieldErrors[0].Property.Should().Be("email");
        response.FieldErrors[1].Code.Should().Be(DefaultErrorCodes.InvalidMin);
        response.FieldErrors[1].Property.Should().Be("age");
        response.FieldErrors[1].RejectedValue.Should().Be(-5);
    }

    // --- Handle: object-level / global errors ---

    [Fact]
    public void Handle_EmptyPropertyName_ProducesGlobalError()
    {
        var failures = new List<ValidationFailure>
        {
            new("", "Object-level validation failed.") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().BeNull();
        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Code.Should().Be(DefaultErrorCodes.RequiredNotEmpty);
        response.GlobalErrors[0].Message.Should().Be("Object-level validation failed.");
    }

    [Fact]
    public void Handle_NullPropertyName_ProducesGlobalError()
    {
        var failure = new ValidationFailure(null!, "Global error") { ErrorCode = "NotNullValidator" };
        var exception = new ValidationException(new[] { failure });

        var response = _handler.Handle(exception);

        response.GlobalErrors.Should().HaveCount(1);
        response.FieldErrors.Should().BeNull();
    }

    // --- Handle: nested property names ---

    [Fact]
    public void Handle_NestedPropertyName_CamelCasesEachSegment()
    {
        var failures = new List<ValidationFailure>
        {
            new("Address.City.ZipCode", "Invalid zip code") { ErrorCode = "NotEmptyValidator", AttemptedValue = "" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Property.Should().Be("address.city.zipCode");
        response.FieldErrors[0].Path.Should().Be("address.city.zipCode");
    }

    // --- Handle: empty Errors collection ---

    [Fact]
    public void Handle_EmptyErrorsCollection_ReturnsGenericValidationFailedResponse()
    {
        var exception = new ValidationException(new List<ValidationFailure>());

        var response = _handler.Handle(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.Message.Should().Be("Validation failed");
        response.FieldErrors.Should().BeNull();
        response.GlobalErrors.Should().BeNull();
    }

    [Fact]
    public void Handle_MessageOnlyException_ReturnsGenericResponse()
    {
        // ValidationException(string message) constructor sets Errors to empty
        var exception = new ValidationException("Some validation message");

        var response = _handler.Handle(exception);

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.FieldErrors.Should().BeNull();
        response.GlobalErrors.Should().BeNull();
    }

    // --- Severity filtering ---

    [Fact]
    public void Handle_DefaultSeverityFilter_ExcludesWarningAndInfo()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Error") { ErrorCode = "NotEmptyValidator", Severity = Severity.Error },
            new("Name", "Warning") { ErrorCode = "NotEmptyValidator", Severity = Severity.Warning },
            new("Age", "Info") { ErrorCode = "NotEmptyValidator", Severity = Severity.Info }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Property.Should().Be("email");
    }

    [Fact]
    public void Handle_IncludeWarning_IncludesErrorAndWarning()
    {
        _fluentOptions.IncludeSeverities.Add(Severity.Warning);

        var failures = new List<ValidationFailure>
        {
            new("Email", "Error") { ErrorCode = "NotEmptyValidator", Severity = Severity.Error },
            new("Name", "Warning") { ErrorCode = "NotEmptyValidator", Severity = Severity.Warning },
            new("Age", "Info") { ErrorCode = "NotEmptyValidator", Severity = Severity.Info }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors.Should().HaveCount(2);
    }

    // --- IncludeRejectedValues option ---

    [Fact]
    public void Handle_IncludeRejectedValuesTrue_IncludesRejectedValue()
    {
        _options.IncludeRejectedValues = true;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "EmailValidator", AttemptedValue = "bad-email" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].RejectedValue.Should().Be("bad-email");
    }

    [Fact]
    public void Handle_IncludeRejectedValuesFalse_ExcludesRejectedValue()
    {
        _options.IncludeRejectedValues = false;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "EmailValidator", AttemptedValue = "bad-email" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].RejectedValue.Should().BeNull();
    }

    // --- AddPathToError option ---

    [Fact]
    public void Handle_AddPathToErrorTrue_IncludesPath()
    {
        _options.AddPathToError = true;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "EmailValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().Be("email");
    }

    [Fact]
    public void Handle_AddPathToErrorFalse_ExcludesPath()
    {
        _options.AddPathToError = false;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "EmailValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Path.Should().BeNull();
    }

    // --- Custom error code mapper respected ---

    [Fact]
    public void Handle_CustomErrorCodeMapper_IsInvoked()
    {
        _errorCodeMapper.GetErrorCode(Arg.Any<string>(), Arg.Any<string>())
            .Returns("CUSTOM_MAPPED_CODE");

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Code.Should().Be("CUSTOM_MAPPED_CODE");
        _errorCodeMapper.Received(1).GetErrorCode(
            Arg.Is<string>(s => s.Contains("Email")),
            DefaultErrorCodes.RequiredNotEmpty);
    }

    // --- Custom error message mapper respected ---

    [Fact]
    public void Handle_CustomErrorMessageMapper_IsInvoked()
    {
        _errorMessageMapper.GetErrorMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("Custom mapped message");

        var failures = new List<ValidationFailure>
        {
            new("Email", "Original message") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.FieldErrors![0].Message.Should().Be("Custom mapped message");
    }

    // --- Handle throws for wrong exception type ---

    [Fact]
    public void Handle_WrongExceptionType_ThrowsInvalidOperationException()
    {
        var exception = new InvalidOperationException("wrong type");

        var act = () => _handler.Handle(exception);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot handle exception*");
    }

    // --- HttpStatusInJsonResponse ---

    [Fact]
    public void Handle_HttpStatusInJsonResponseTrue_IncludesStatus()
    {
        _options.HttpStatusInJsonResponse = true;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.Status.Should().Be(400);
    }

    [Fact]
    public void Handle_HttpStatusInJsonResponseFalse_ExcludesStatus()
    {
        _options.HttpStatusInJsonResponse = false;

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        response.Status.Should().Be(0);
    }

    // --- User-defined custom error codes preserved ---

    [Fact]
    public void Handle_UserDefinedErrorCode_PreservedExactly()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Custom error") { ErrorCode = "EMAIL_REQUIRED", AttemptedValue = "" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        // EMAIL_REQUIRED is not in the mapping, so it passes through as-is
        response.FieldErrors![0].Code.Should().Be("EMAIL_REQUIRED");
    }
}
