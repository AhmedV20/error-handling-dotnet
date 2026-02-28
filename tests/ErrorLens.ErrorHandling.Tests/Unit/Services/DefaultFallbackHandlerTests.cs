using System.Net;
using ErrorLens.ErrorHandling.Attributes;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Services;

public class DefaultFallbackHandlerTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly DefaultFallbackHandler _handler;

    public DefaultFallbackHandlerTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);

        var codeMapper = new ErrorCodeMapper(optionsWrapper);
        var msgMapper = new ErrorMessageMapper(optionsWrapper);
        var statusMapper = new HttpStatusMapper(optionsWrapper);

        _handler = new DefaultFallbackHandler(codeMapper, msgMapper, statusMapper, optionsWrapper);
    }

    [Fact]
    public void Handle_UnknownException_Returns500WithSafeMessage()
    {
        var exception = new Exception("Internal details: connection string=...");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Code.Should().Be("INTERNAL_ERROR");
        response.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void Handle_4xxException_PreservesOriginalMessage()
    {
        var exception = new ArgumentException("Invalid argument provided");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Message.Should().Be("Invalid argument provided");
    }

    [Fact]
    public void Handle_WithResponseErrorCode_UsesAttributeCode()
    {
        var exception = new TestCodeException();

        var response = _handler.Handle(exception);

        response.Code.Should().Be("CUSTOM_ERROR");
    }

    [Fact]
    public void Handle_WithResponseStatus_UsesAttributeStatus()
    {
        var exception = new TestStatusException();

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Handle_WithResponseErrorProperty_IncludesProperty()
    {
        var exception = new TestPropertyException("user-123");

        var response = _handler.Handle(exception);

        response.Properties.Should().ContainKey("userId");
        response.Properties!["userId"].Should().Be("user-123");
    }

    [Fact]
    public void Handle_5xxWithAttributeStatus_StillSanitizesMessage()
    {
        var exception = new Test5xxException("Sensitive server details");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void Handle_HttpStatusInJsonResponse_SetsStatus()
    {
        _options.HttpStatusInJsonResponse = true;
        var exception = new KeyNotFoundException("not found");

        var response = _handler.Handle(exception);

        response.Status.Should().Be(404);
    }

    // Test exceptions

    [ResponseErrorCode("CUSTOM_ERROR")]
    private class TestCodeException : Exception { }

    [ResponseStatus(HttpStatusCode.NotFound)]
    private class TestStatusException : Exception { }

    private class TestPropertyException : Exception
    {
        [ResponseErrorProperty("userId")]
        public string UserId { get; }

        public TestPropertyException(string userId) : base("Test")
        {
            UserId = userId;
        }
    }

    // US2: FallbackMessage tests

    [Fact]
    public void Handle_CustomFallbackMessage_UsedFor5xxResponses()
    {
        _options.FallbackMessage = "Contact support at help@example.com";
        var exception = new Exception("Internal details: connection string=...");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("Contact support at help@example.com");
    }

    [Fact]
    public void Handle_DefaultFallbackMessage_UsedWhenNotConfigured()
    {
        // FallbackMessage defaults to "An unexpected error occurred"
        var exception = new Exception("Server crash details");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void Handle_4xxException_NotAffectedByFallbackMessage()
    {
        _options.FallbackMessage = "Custom server error message";
        var exception = new ArgumentException("Invalid argument provided");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Message.Should().Be("Invalid argument provided");
    }

    [Fact]
    public void Handle_5xxWithCustomFallbackMessage_OverridesDefault()
    {
        _options.FallbackMessage = "Something went wrong. Please try again later.";
        var exception = new Test5xxException("Sensitive server details");

        var response = _handler.Handle(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Message.Should().Be("Something went wrong. Please try again later.");
    }

    [ResponseStatus(HttpStatusCode.ServiceUnavailable)]
    private class Test5xxException : Exception
    {
        public Test5xxException(string message) : base(message) { }
    }
}
