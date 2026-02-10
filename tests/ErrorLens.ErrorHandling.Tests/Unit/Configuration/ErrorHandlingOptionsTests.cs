using ErrorLens.ErrorHandling.Configuration;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Configuration;

public class ErrorHandlingOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new ErrorHandlingOptions();

        options.Enabled.Should().BeTrue();
        options.DefaultErrorCodeStrategy.Should().Be(ErrorCodeStrategy.AllCaps);
        options.HttpStatusInJsonResponse.Should().BeFalse();
        options.SearchSuperClassHierarchy.Should().BeFalse();
        options.AddPathToError.Should().BeTrue();
        options.UseProblemDetailFormat.Should().BeFalse();
        options.ProblemDetailConvertToKebabCase.Should().BeTrue();
        options.ExceptionLogging.Should().Be(ExceptionLogging.MessageOnly);
    }

    [Fact]
    public void SectionName_IsErrorHandling()
    {
        ErrorHandlingOptions.SectionName.Should().Be("ErrorHandling");
    }

    [Fact]
    public void HttpStatuses_DefaultsToEmptyDictionary()
    {
        var options = new ErrorHandlingOptions();

        options.HttpStatuses.Should().NotBeNull();
        options.HttpStatuses.Should().BeEmpty();
    }

    [Fact]
    public void Codes_DefaultsToEmptyDictionary()
    {
        var options = new ErrorHandlingOptions();

        options.Codes.Should().NotBeNull();
        options.Codes.Should().BeEmpty();
    }

    [Fact]
    public void Messages_DefaultsToEmptyDictionary()
    {
        var options = new ErrorHandlingOptions();

        options.Messages.Should().NotBeNull();
        options.Messages.Should().BeEmpty();
    }

    [Fact]
    public void JsonFieldNames_HasDefaultValues()
    {
        var options = new ErrorHandlingOptions();

        options.JsonFieldNames.Code.Should().Be("code");
        options.JsonFieldNames.Message.Should().Be("message");
        options.JsonFieldNames.FieldErrors.Should().Be("fieldErrors");
        options.JsonFieldNames.GlobalErrors.Should().Be("globalErrors");
        options.JsonFieldNames.ParameterErrors.Should().Be("parameterErrors");
    }

    [Fact]
    public void ProblemDetailTypePrefix_HasDefaultValue()
    {
        var options = new ErrorHandlingOptions();

        options.ProblemDetailTypePrefix.Should().Be("https://example.com/errors/");
    }
}
