using ErrorLens.ErrorHandling.Localization;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Localization;

public class NoOpErrorMessageLocalizerTests
{
    private readonly NoOpErrorMessageLocalizer _localizer = new();

    [Fact]
    public void Localize_ReturnsDefaultMessage_Unchanged()
    {
        var result = _localizer.Localize("VALIDATION_FAILED", "Validation failed");

        result.Should().Be("Validation failed");
    }

    [Fact]
    public void Localize_WithNullDefaultMessage_ReturnsNull()
    {
        var result = _localizer.Localize("VALIDATION_FAILED", null);

        result.Should().BeNull();
    }

    [Fact]
    public void Localize_WithEmptyDefaultMessage_ReturnsEmpty()
    {
        var result = _localizer.Localize("VALIDATION_FAILED", "");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Localize_IgnoresErrorCode_ReturnsDefaultMessage()
    {
        var result = _localizer.Localize("ANY_CODE", "Some message");

        result.Should().Be("Some message");
    }

    [Fact]
    public void LocalizeFieldError_ReturnsDefaultMessage_Unchanged()
    {
        var result = _localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "email", "Email is required");

        result.Should().Be("Email is required");
    }

    [Fact]
    public void LocalizeFieldError_WithNullDefaultMessage_ReturnsNull()
    {
        var result = _localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "email", null);

        result.Should().BeNull();
    }

    [Fact]
    public void LocalizeFieldError_IgnoresFieldName_ReturnsDefaultMessage()
    {
        var result = _localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "anyField", "Field is required");

        result.Should().Be("Field is required");
    }
}
