using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Contract;

public class FluentValidationResponseFormatTests
{
    private readonly FluentValidationExceptionHandler _handler;

    public FluentValidationResponseFormatTests()
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var fluentOptions = new FluentValidationOptions();
        var fluentOptionsWrapper = Substitute.For<IOptions<FluentValidationOptions>>();
        fluentOptionsWrapper.Value.Returns(fluentOptions);

        var errorCodeMapper = Substitute.For<IErrorCodeMapper>();
        errorCodeMapper.GetErrorCode(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(1));

        var errorMessageMapper = Substitute.For<IErrorMessageMapper>();
        errorMessageMapper.GetErrorMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(2));

        _handler = new FluentValidationExceptionHandler(
            errorCodeMapper, errorMessageMapper, optionsWrapper, fluentOptionsWrapper);
    }

    [Fact]
    public void Handle_FieldErrors_MatchesExpectedJsonStructure()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.")
            {
                ErrorCode = "NotEmptyValidator",
                AttemptedValue = ""
            },
            new("Age", "'Age' must be greater than '0'.")
            {
                ErrorCode = "GreaterThanValidator",
                AttemptedValue = -5
            }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        // Serialize to JSON to verify structure
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");
        root.GetProperty("message").GetString().Should().Be("Validation failed");

        var fieldErrors = root.GetProperty("fieldErrors");
        fieldErrors.GetArrayLength().Should().Be(2);

        var firstError = fieldErrors[0];
        firstError.GetProperty("code").GetString().Should().Be("REQUIRED_NOT_EMPTY");
        firstError.GetProperty("property").GetString().Should().Be("email");
        firstError.GetProperty("message").GetString().Should().Be("'Email' must not be empty.");
        firstError.GetProperty("rejectedValue").GetString().Should().Be("");
        firstError.GetProperty("path").GetString().Should().Be("email");

        var secondError = fieldErrors[1];
        secondError.GetProperty("code").GetString().Should().Be("VALUE_TOO_LOW");
        secondError.GetProperty("property").GetString().Should().Be("age");
        secondError.GetProperty("message").GetString().Should().Be("'Age' must be greater than '0'.");
        secondError.GetProperty("rejectedValue").GetInt32().Should().Be(-5);
        secondError.GetProperty("path").GetString().Should().Be("age");
    }

    [Fact]
    public void Handle_GlobalErrors_MatchesExpectedJsonStructure()
    {
        var failures = new List<ValidationFailure>
        {
            new("", "Object-level validation failed.") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = _handler.Handle(exception);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");

        var globalErrors = root.GetProperty("globalErrors");
        globalErrors.GetArrayLength().Should().Be(1);
        globalErrors[0].GetProperty("code").GetString().Should().Be("REQUIRED_NOT_EMPTY");
        globalErrors[0].GetProperty("message").GetString().Should().Be("Object-level validation failed.");

        // fieldErrors should not be present
        root.TryGetProperty("fieldErrors", out _).Should().BeFalse();
    }

    [Fact]
    public void Handle_EmptyErrors_ProducesMinimalResponse()
    {
        var exception = new ValidationException(new List<ValidationFailure>());

        var response = _handler.Handle(exception);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");
        root.GetProperty("message").GetString().Should().Be("Validation failed");
        root.TryGetProperty("fieldErrors", out _).Should().BeFalse();
        root.TryGetProperty("globalErrors", out _).Should().BeFalse();
    }
}
