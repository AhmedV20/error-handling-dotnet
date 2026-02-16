using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Mappers;

public class ErrorMessageMapperTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ErrorMessageMapper _mapper;

    public ErrorMessageMapperTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _mapper = new ErrorMessageMapper(optionsWrapper);
    }

    [Fact]
    public void GetErrorMessage_NoOverride_ReturnsExceptionMessage()
    {
        var exception = new InvalidOperationException("Something went wrong");

        var message = _mapper.GetErrorMessage(exception);

        message.Should().Be("Something went wrong");
    }

    [Fact]
    public void GetErrorMessage_WithConfiguredOverride_ReturnsConfiguredMessage()
    {
        _options.Messages["System.InvalidOperationException"] = "An invalid operation occurred";
        var exception = new InvalidOperationException("raw message");

        var message = _mapper.GetErrorMessage(exception);

        message.Should().Be("An invalid operation occurred");
    }

    [Fact]
    public void GetErrorMessage_WithSuperClassHierarchy_FindsBaseClassMessage()
    {
        _options.SearchSuperClassHierarchy = true;
        _options.Messages["System.SystemException"] = "System error occurred";
        var exception = new InvalidOperationException("raw");

        var message = _mapper.GetErrorMessage(exception);

        message.Should().Be("System error occurred");
    }

    [Fact]
    public void GetErrorMessage_FieldSpecific_ReturnsFieldOverride()
    {
        _options.Messages["email.Required"] = "A valid email is required";

        var message = _mapper.GetErrorMessage("email.Required", "REQUIRED_NOT_NULL", "The Email field is required.");

        message.Should().Be("A valid email is required");
    }

    [Fact]
    public void GetErrorMessage_FieldSpecific_FallsBackToDefault()
    {
        var message = _mapper.GetErrorMessage("email.Required", "REQUIRED_NOT_NULL", "The Email field is required.");

        message.Should().Be("The Email field is required.");
    }

    [Fact]
    public void GetErrorMessage_CodeSpecific_ReturnsCodeOverride()
    {
        _options.Messages["REQUIRED_NOT_NULL"] = "This field cannot be empty";

        var message = _mapper.GetErrorMessage("email.Required", "REQUIRED_NOT_NULL", "default message");

        message.Should().Be("This field cannot be empty");
    }
}
