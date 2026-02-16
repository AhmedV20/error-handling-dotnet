using System.Net;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class BadRequestExceptionHandlerTests
{
    private readonly BadRequestExceptionHandler _handler = new();

    [Fact]
    public void Order_Is150()
    {
        _handler.Order.Should().Be(150);
    }

    [Fact]
    public void CanHandle_BadHttpRequestException_ReturnsTrue()
    {
        _handler.CanHandle(new BadHttpRequestException("test")).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        _handler.CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Handle_WrongType_ThrowsInvalidOperationException()
    {
        var act = () => _handler.Handle(new InvalidOperationException());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Handle_SafeMessage_PreservesMessage()
    {
        var exception = new BadHttpRequestException("Missing content type");

        var response = _handler.Handle(exception);

        response.Code.Should().Be("BAD_REQUEST");
        response.Message.Should().Be("Missing content type");
    }

    [Fact]
    public void Handle_MessageWithMicrosoft_Sanitizes()
    {
        var exception = new BadHttpRequestException("Microsoft.AspNetCore.Http failed to bind");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Bad request");
    }

    [Fact]
    public void Handle_MessageWithSystem_Sanitizes()
    {
        var exception = new BadHttpRequestException("System.Text.Json deserialization failed");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Bad request");
    }

    [Fact]
    public void Handle_MessageWithFailedToRead_Sanitizes()
    {
        var exception = new BadHttpRequestException("Failed to read the request body");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Bad request");
    }

    [Fact]
    public void Handle_MessageWithUnexpectedEnd_Sanitizes()
    {
        var exception = new BadHttpRequestException("Unexpected end of request content");

        var response = _handler.Handle(exception);

        response.Message.Should().Be("Bad request");
    }

    [Fact]
    public void Handle_UsesActualStatusCode()
    {
        var exception = new BadHttpRequestException("test", 413);

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be((HttpStatusCode)413);
    }
}
