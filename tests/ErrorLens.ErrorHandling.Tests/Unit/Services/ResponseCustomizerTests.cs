using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Services;

public class ResponseCustomizerTests
{
    [Fact]
    public void Customizer_AddsProperty_ToResponse()
    {
        var customizer = new TimestampCustomizer();
        var response = new ApiErrorResponse("ERROR", "Test");

        customizer.Customize(response);

        response.Properties.Should().ContainKey("timestamp");
    }

    [Fact]
    public void MultipleCustomizers_AllApplied()
    {
        var customizer1 = new TimestampCustomizer();
        var customizer2 = new TraceIdCustomizer();
        var response = new ApiErrorResponse("ERROR", "Test");

        customizer1.Customize(response);
        customizer2.Customize(response);

        response.Properties.Should().ContainKey("timestamp");
        response.Properties.Should().ContainKey("traceId");
    }

    [Fact]
    public void Facade_AppliesAllCustomizers()
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper,
            errorMessageMapper,
            httpStatusMapper,
            optionsWrapper);

        var customizers = new IApiErrorResponseCustomizer[]
        {
            new TimestampCustomizer(),
            new TraceIdCustomizer()
        };

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            customizers,
            optionsWrapper,
            logger);

        var response = facade.HandleException(new Exception("test"));

        response.Properties.Should().ContainKey("timestamp");
        response.Properties.Should().ContainKey("traceId");
    }

    private class TimestampCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
        }
    }

    private class TraceIdCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("traceId", Guid.NewGuid().ToString());
        }
    }
}
