using ErrorLens.ErrorHandling.Localization;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Localization;

public class StringLocalizerErrorMessageLocalizerTests
{
    private readonly IStringLocalizer<TestResource> _stringLocalizer;
    private readonly StringLocalizerErrorMessageLocalizer<TestResource> _localizer;

    public StringLocalizerErrorMessageLocalizerTests()
    {
        _stringLocalizer = Substitute.For<IStringLocalizer<TestResource>>();
        _localizer = new StringLocalizerErrorMessageLocalizer<TestResource>(_stringLocalizer);
    }

    [Fact]
    public void Localize_WhenResourceFound_ReturnsLocalizedMessage()
    {
        _stringLocalizer["VALIDATION_FAILED"]
            .Returns(new LocalizedString("VALIDATION_FAILED", "Validation hat fehlgeschlagen", false));

        var result = _localizer.Localize("VALIDATION_FAILED", "Validation failed");

        result.Should().Be("Validation hat fehlgeschlagen");
    }

    [Fact]
    public void Localize_WhenResourceNotFound_ReturnsDefaultMessage()
    {
        _stringLocalizer["UNKNOWN_CODE"]
            .Returns(new LocalizedString("UNKNOWN_CODE", "UNKNOWN_CODE", true));

        var result = _localizer.Localize("UNKNOWN_CODE", "Default message");

        result.Should().Be("Default message");
    }

    [Fact]
    public void Localize_WhenErrorCodeIsNull_ReturnsDefaultMessage()
    {
        var result = _localizer.Localize(null!, "Default message");

        result.Should().Be("Default message");
    }

    [Fact]
    public void Localize_WhenDefaultMessageIsNull_ReturnsNull()
    {
        _stringLocalizer["SOME_CODE"]
            .Returns(new LocalizedString("SOME_CODE", "SOME_CODE", true));

        var result = _localizer.Localize("SOME_CODE", null);

        result.Should().BeNull();
    }

    [Fact]
    public void LocalizeFieldError_WhenResourceFound_ReturnsLocalizedMessage()
    {
        // Composite key not found, falls back to error code
        _stringLocalizer["email.REQUIRED_NOT_NULL"]
            .Returns(new LocalizedString("email.REQUIRED_NOT_NULL", "email.REQUIRED_NOT_NULL", true));
        _stringLocalizer["REQUIRED_NOT_NULL"]
            .Returns(new LocalizedString("REQUIRED_NOT_NULL", "Pflichtfeld", false));

        var result = _localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "email", "Required field");

        result.Should().Be("Pflichtfeld");
    }

    [Fact]
    public void LocalizeFieldError_WhenFieldSpecificResourceFound_ReturnsFieldSpecificMessage()
    {
        // Composite key found — should use field-specific translation
        _stringLocalizer["email.REQUIRED_NOT_NULL"]
            .Returns(new LocalizedString("email.REQUIRED_NOT_NULL", "E-Mail ist erforderlich", false));

        var result = _localizer.LocalizeFieldError("REQUIRED_NOT_NULL", "email", "Required field");

        result.Should().Be("E-Mail ist erforderlich");
    }

    [Fact]
    public void LocalizeFieldError_WhenResourceNotFound_ReturnsDefaultMessage()
    {
        _stringLocalizer["email.UNKNOWN_FIELD_CODE"]
            .Returns(new LocalizedString("email.UNKNOWN_FIELD_CODE", "email.UNKNOWN_FIELD_CODE", true));
        _stringLocalizer["UNKNOWN_FIELD_CODE"]
            .Returns(new LocalizedString("UNKNOWN_FIELD_CODE", "UNKNOWN_FIELD_CODE", true));

        var result = _localizer.LocalizeFieldError("UNKNOWN_FIELD_CODE", "email", "Field is invalid");

        result.Should().Be("Field is invalid");
    }

    [Fact]
    public void LocalizeFieldError_UsesErrorCodeAsResourceKey()
    {
        // Composite key not found, falls back to error code
        _stringLocalizer["userEmail.INVALID_EMAIL"]
            .Returns(new LocalizedString("userEmail.INVALID_EMAIL", "userEmail.INVALID_EMAIL", true));
        _stringLocalizer["INVALID_EMAIL"]
            .Returns(new LocalizedString("INVALID_EMAIL", "E-Mail ungültig", false));

        _localizer.LocalizeFieldError("INVALID_EMAIL", "userEmail", "Invalid email");

        // Verify it looked up by composite key first, then error code
        _ = _stringLocalizer.Received(1)["userEmail.INVALID_EMAIL"];
        _ = _stringLocalizer.Received(1)["INVALID_EMAIL"];
    }

    [Fact]
    public void LocalizeFieldError_WhenErrorCodeIsNull_ReturnsDefaultMessage()
    {
        var result = _localizer.LocalizeFieldError(null!, "email", "Default field message");

        result.Should().Be("Default field message");
    }
}

public class TestResource { }
