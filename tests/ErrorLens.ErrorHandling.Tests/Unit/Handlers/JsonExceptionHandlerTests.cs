using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class JsonExceptionHandlerTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly JsonExceptionHandler _handler;

    public JsonExceptionHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _handler = new JsonExceptionHandler(optionsWrapper);
    }

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

    // US3: BuiltInMessages tests

    [Fact]
    public void Handle_CustomBuiltInMessage_UsesCustomMessage()
    {
        _options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "Invalid JSON in request body";

        var response = _handler.Handle(new JsonException("parse error"));

        response.Code.Should().Be("MESSAGE_NOT_READABLE");
        response.Message.Should().Be("Invalid JSON in request body");
    }

    [Fact]
    public void Handle_DefaultMessage_WhenKeyNotInBuiltInMessages()
    {
        // BuiltInMessages is empty by default
        var response = _handler.Handle(new JsonException("parse error"));

        response.Message.Should().Be("The request body could not be parsed as valid JSON");
    }

    [Fact]
    public void Handle_EmptyStringBuiltInMessage_UsesEmptyString()
    {
        _options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "";

        var response = _handler.Handle(new JsonException("parse error"));

        response.Message.Should().Be("");
    }

    [Fact]
    public void Handle_CustomBuiltInMessage_PreservesCodeAndStatus()
    {
        _options.BuiltInMessages["MESSAGE_NOT_READABLE"] = "Custom message";

        var response = _handler.Handle(new JsonException("parse error"));

        response.Code.Should().Be("MESSAGE_NOT_READABLE");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
