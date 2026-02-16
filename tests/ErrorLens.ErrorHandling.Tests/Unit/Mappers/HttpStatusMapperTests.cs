using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Mappers;

public class HttpStatusMapperTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly HttpStatusMapper _mapper;

    public HttpStatusMapperTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _mapper = new HttpStatusMapper(optionsWrapper);
    }

    [Fact]
    public void GetHttpStatus_UnknownException_ReturnsInternalServerError()
    {
        var exception = new Exception("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void GetHttpStatus_ArgumentException_ReturnsBadRequest()
    {
        var exception = new ArgumentException("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetHttpStatus_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var exception = new UnauthorizedAccessException("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void GetHttpStatus_KeyNotFoundException_ReturnsNotFound()
    {
        var exception = new KeyNotFoundException("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void GetHttpStatus_WithConfiguredOverride_ReturnsConfiguredStatus()
    {
        _options.HttpStatuses["System.Exception"] = HttpStatusCode.ServiceUnavailable;
        var exception = new Exception("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public void GetHttpStatus_WithDefaultStatus_ReturnsDefaultWhenNoMatch()
    {
        var exception = new Exception("test");

        var status = _mapper.GetHttpStatus(exception, HttpStatusCode.BadGateway);

        status.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public void GetHttpStatus_TimeoutException_ReturnsRequestTimeout()
    {
        var exception = new TimeoutException("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    public void GetHttpStatus_NotImplementedException_ReturnsNotImplemented()
    {
        var exception = new NotImplementedException("test");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be(HttpStatusCode.NotImplemented);
    }

    // --- OperationCanceledException → 499 test (T049) ---

    [Fact]
    public void GetHttpStatus_OperationCanceledException_Returns499()
    {
        var exception = new OperationCanceledException("cancelled");

        var status = _mapper.GetHttpStatus(exception);

        status.Should().Be((HttpStatusCode)499);
    }

    [Fact]
    public void GetHttpStatus_TaskCanceledException_Returns499()
    {
        var exception = new TaskCanceledException("cancelled");

        var status = _mapper.GetHttpStatus(exception);

        // TaskCanceledException extends OperationCanceledException
        status.Should().Be((HttpStatusCode)499);
    }
}
