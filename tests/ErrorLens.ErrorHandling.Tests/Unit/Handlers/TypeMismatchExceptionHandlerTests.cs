using System.Net;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class TypeMismatchExceptionHandlerTests
{
    private readonly TypeMismatchExceptionHandler _handler = new();

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
}
