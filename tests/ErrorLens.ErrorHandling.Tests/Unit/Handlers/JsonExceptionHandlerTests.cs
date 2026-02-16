using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class JsonExceptionHandlerTests
{
    private readonly JsonExceptionHandler _handler = new();

    [Fact]
    public void Order_Is120()
    {
        _handler.Order.Should().Be(120);
    }

    [Fact]
    public void CanHandle_JsonException_ReturnsTrue()
    {
        _handler.CanHandle(new JsonException("Invalid JSON")).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        _handler.CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Handle_ReturnsMessageNotReadable()
    {
        var response = _handler.Handle(new JsonException("'a' is an invalid start"));

        response.Code.Should().Be("MESSAGE_NOT_READABLE");
        response.Message.Should().Be("The request body could not be parsed as valid JSON");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
