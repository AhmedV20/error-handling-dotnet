using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.FluentValidation;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Unit;

public class FluentValidationErrorCodeMappingTests
{
    [Theory]
    [InlineData("NotNullValidator", "REQUIRED_NOT_NULL")]
    [InlineData("NotEmptyValidator", "REQUIRED_NOT_EMPTY")]
    [InlineData("EmailValidator", "INVALID_EMAIL")]
    [InlineData("LengthValidator", "INVALID_SIZE")]
    [InlineData("MinimumLengthValidator", "INVALID_SIZE")]
    [InlineData("MaximumLengthValidator", "INVALID_SIZE")]
    [InlineData("LessThanValidator", "VALUE_TOO_HIGH")]
    [InlineData("LessThanOrEqualValidator", "VALUE_TOO_HIGH")]
    [InlineData("GreaterThanValidator", "VALUE_TOO_LOW")]
    [InlineData("GreaterThanOrEqualValidator", "VALUE_TOO_LOW")]
    [InlineData("RegularExpressionValidator", "REGEX_PATTERN_VALIDATION_FAILED")]
    [InlineData("CreditCardValidator", "INVALID_CREDIT_CARD")]
    [InlineData("InclusiveBetweenValidator", "VALUE_OUT_OF_RANGE")]
    [InlineData("ExclusiveBetweenValidator", "VALUE_OUT_OF_RANGE")]
    public void MapErrorCode_KnownValidator_ReturnsMappedCode(string validatorCode, string expectedCode)
    {
        var result = FluentValidationErrorCodeMapping.MapErrorCode(validatorCode);
        result.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("UniqueValidator")]
    [InlineData("CustomValidator")]
    [InlineData("SomeThirdPartyValidator")]
    public void MapErrorCode_UnknownValidator_ReturnsAsIs(string validatorCode)
    {
        var result = FluentValidationErrorCodeMapping.MapErrorCode(validatorCode);
        result.Should().Be(validatorCode);
    }

    [Theory]
    [InlineData("CUSTOM_CODE")]
    [InlineData("EMAIL_REQUIRED")]
    [InlineData("MY_APP_ERROR")]
    public void MapErrorCode_UserDefinedCustomCode_PreservedExactly(string customCode)
    {
        var result = FluentValidationErrorCodeMapping.MapErrorCode(customCode);
        result.Should().Be(customCode);
    }

    [Fact]
    public void MapErrorCode_AllFourteenMappings_ArePresent()
    {
        // Verify the complete set of 14 known mappings
        var knownValidators = new[]
        {
            "NotNullValidator", "NotEmptyValidator", "EmailValidator",
            "LengthValidator", "MinimumLengthValidator", "MaximumLengthValidator",
            "LessThanValidator", "LessThanOrEqualValidator",
            "GreaterThanValidator", "GreaterThanOrEqualValidator",
            "RegularExpressionValidator", "CreditCardValidator",
            "InclusiveBetweenValidator", "ExclusiveBetweenValidator"
        };

        foreach (var validator in knownValidators)
        {
            var result = FluentValidationErrorCodeMapping.MapErrorCode(validator);
            result.Should().NotBe(validator, because: $"{validator} should be mapped to a DefaultErrorCodes constant");
        }
    }
}
