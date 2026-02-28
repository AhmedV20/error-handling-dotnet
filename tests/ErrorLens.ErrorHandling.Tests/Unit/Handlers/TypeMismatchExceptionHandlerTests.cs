using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class TypeMismatchExceptionHandlerTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly TypeMismatchExceptionHandler _handler;

    public TypeMismatchExceptionHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _handler = new TypeMismatchExceptionHandler(optionsWrapper);
    }

    [Fact]
    public void Order_Is130()
    {
        _handler.Order.Should().Be(130);
    }

    [Fact]
    public void CanHandle_FormatException_ReturnsTrue()
    {
        _handler.CanHandle(new FormatException()).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_InvalidCastException_ReturnsTrue()
    {
        _handler.CanHandle(new InvalidCastException()).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        _handler.CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Handle_FormatException_ReturnsGenericMessage()
    {
        var response = _handler.Handle(new FormatException("Input string was not in a correct format."));

        response.Code.Should().Be("TYPE_MISMATCH");
        response.Message.Should().Be("A type conversion error occurred");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Handle_InvalidCastException_ReturnsGenericMessage()
    {
        var response = _handler.Handle(new InvalidCastException("Cannot cast from string to int"));

        response.Message.Should().Be("A type conversion error occurred");
    }

    [Fact]
    public void Handle_WrongType_ThrowsInvalidOperationException()
    {
        var act = () => _handler.Handle(new InvalidOperationException());

        act.Should().Throw<InvalidOperationException>();
    }

    // US3: BuiltInMessages tests

    [Fact]
    public void Handle_CustomBuiltInMessage_UsesCustomMessage()
    {
        _options.BuiltInMessages["TYPE_MISMATCH"] = "Value type does not match expected type";

        var response = _handler.Handle(new FormatException("bad format"));

        response.Code.Should().Be("TYPE_MISMATCH");
        response.Message.Should().Be("Value type does not match expected type");
    }

    [Fact]
    public void Handle_DefaultMessage_WhenKeyNotInBuiltInMessages()
    {
        var response = _handler.Handle(new FormatException("bad format"));

        response.Message.Should().Be("A type conversion error occurred");
    }

    [Fact]
    public void Handle_EmptyStringBuiltInMessage_UsesEmptyString()
    {
        _options.BuiltInMessages["TYPE_MISMATCH"] = "";

        var response = _handler.Handle(new FormatException("bad format"));

        response.Message.Should().Be("");
    }
}
