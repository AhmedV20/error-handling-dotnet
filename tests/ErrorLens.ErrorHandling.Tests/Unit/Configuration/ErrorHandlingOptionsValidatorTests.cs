using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.RateLimiting;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Configuration;

public class ErrorHandlingOptionsValidatorTests
{
    private static ValidateOptionsResult ValidateOptions(ErrorHandlingOptions options)
    {
        var validatorType = typeof(ErrorHandlingOptions).Assembly
            .GetType("ErrorLens.ErrorHandling.Configuration.ErrorHandlingOptionsValidator")!;
        var validator = Activator.CreateInstance(validatorType)!;
        var validateMethod = validatorType.GetMethod("Validate")!;
        return (ValidateOptionsResult)validateMethod.Invoke(validator, new object?[] { null, options })!;
    }

    // --- ProblemDetailTypePrefix validation ---

    [Fact]
    public void Validate_ValidAbsoluteUri_Succeeds()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "https://example.com/errors/"
        };

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyProblemDetailTypePrefix_Succeeds()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = ""
        };

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidUri_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "not a uri"
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ProblemDetailTypePrefix");
        result.FailureMessage.Should().Contain("not a uri");
        result.FailureMessage.Should().Contain("valid URI");
    }

    [Fact]
    public void Validate_RelativeUri_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "/errors/"
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ProblemDetailTypePrefix");
    }

    [Fact]
    public void Validate_HttpUri_Succeeds()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "http://api.example.com/problems/"
        };

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    // --- RateLimiting.ErrorCode validation ---

    [Fact]
    public void Validate_NonEmptyErrorCode_Succeeds()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { ErrorCode = "THROTTLED" }
        };

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullErrorCode_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { ErrorCode = null! }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.ErrorCode");
    }

    [Fact]
    public void Validate_EmptyErrorCode_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { ErrorCode = "" }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.ErrorCode");
    }

    [Fact]
    public void Validate_WhitespaceErrorCode_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { ErrorCode = "   " }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.ErrorCode");
    }

    // --- RateLimiting.DefaultMessage validation ---

    [Fact]
    public void Validate_NonEmptyDefaultMessage_Succeeds()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { DefaultMessage = "Slow down!" }
        };

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullDefaultMessage_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { DefaultMessage = null! }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.DefaultMessage");
    }

    [Fact]
    public void Validate_EmptyDefaultMessage_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { DefaultMessage = "" }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.DefaultMessage");
    }

    [Fact]
    public void Validate_WhitespaceDefaultMessage_Fails()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { DefaultMessage = "   " }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimiting.DefaultMessage");
    }

    // --- Existing validation unchanged ---

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var options = new ErrorHandlingOptions();

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ExistingJsonFieldNames_StillValidated()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = null!;

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Code");
    }

    [Fact]
    public void Validate_MultipleFailures_ReportsAll()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "not a uri",
            RateLimiting = new RateLimitingOptions
            {
                ErrorCode = "",
                DefaultMessage = ""
            }
        };

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ProblemDetailTypePrefix");
        result.FailureMessage.Should().Contain("RateLimiting.ErrorCode");
        result.FailureMessage.Should().Contain("RateLimiting.DefaultMessage");
    }
}
