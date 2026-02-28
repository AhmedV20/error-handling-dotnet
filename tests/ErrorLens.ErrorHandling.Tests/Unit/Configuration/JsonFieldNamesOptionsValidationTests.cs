using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Configuration;

public class JsonFieldNamesOptionsValidationTests
{
    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var options = new ErrorHandlingOptions();

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullCodeProperty_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = null!;

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Code");
    }

    [Fact]
    public void Validate_EmptyMessageProperty_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Message = "";

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Message");
    }

    [Fact]
    public void Validate_WhitespaceStatusProperty_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Status = "   ";

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Status");
    }

    [Fact]
    public void Validate_DuplicateFieldNames_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = "type";
        options.JsonFieldNames.Message = "type"; // duplicate!

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Duplicate");
        result.FailureMessage.Should().Contain("type");
    }

    [Fact]
    public void Validate_AllCustomNamesUnique_Succeeds()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = "type";
        options.JsonFieldNames.Message = "detail";
        options.JsonFieldNames.Status = "statusCode";
        options.JsonFieldNames.FieldErrors = "fields";
        options.JsonFieldNames.GlobalErrors = "errors";
        options.JsonFieldNames.ParameterErrors = "params";
        options.JsonFieldNames.Property = "field";
        options.JsonFieldNames.RejectedValue = "value";
        options.JsonFieldNames.Path = "jsonPath";
        options.JsonFieldNames.Parameter = "paramName";

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultipleNullProperties_ReportsAllFailures()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = null!;
        options.JsonFieldNames.Message = null!;
        options.JsonFieldNames.Path = null!;

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Code");
        result.FailureMessage.Should().Contain("Message");
        result.FailureMessage.Should().Contain("Path");
    }

    [Fact]
    public void Validate_ViaServiceProvider_RejectsInvalidOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddErrorHandling(opts =>
        {
            opts.JsonFieldNames.Code = null!;
        });

        var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<ErrorHandlingOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    // --- US5: RetryAfter validation tests ---

    [Fact]
    public void Validate_NullRetryAfter_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.RetryAfter = null!;

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RetryAfter");
    }

    [Fact]
    public void Validate_EmptyRetryAfter_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.RetryAfter = "";

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RetryAfter");
    }

    [Fact]
    public void Validate_CustomRetryAfter_Succeeds()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.RetryAfter = "retry_after";

        var result = ValidateOptions(options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_RetryAfterDuplicatesCode_Fails()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.RetryAfter = "code"; // duplicates Code field

        var result = ValidateOptions(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Duplicate");
        result.FailureMessage.Should().Contain("code");
    }

    private static ValidateOptionsResult ValidateOptions(ErrorHandlingOptions options)
    {
        // Use reflection to instantiate the internal validator
        var validatorType = typeof(ErrorHandlingOptions).Assembly
            .GetType("ErrorLens.ErrorHandling.Configuration.ErrorHandlingOptionsValidator")!;
        var validator = Activator.CreateInstance(validatorType)!;
        var validateMethod = validatorType.GetMethod("Validate")!;
        return (ValidateOptionsResult)validateMethod.Invoke(validator, new object?[] { null, options })!;
    }
}
