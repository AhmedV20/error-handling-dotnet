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

namespace ErrorLens.ErrorHandling.Tests.Integration;

/// <summary>
/// Integration tests for response customization.
/// </summary>
public class ResponseCustomizationTests
{
    [Fact]
    public void MultipleCustomizers_AppliedInOrder()
    {
        var options = new ErrorHandlingOptions();
        var customizers = new IApiErrorResponseCustomizer[]
        {
            new FirstCustomizer(),
            new SecondCustomizer()
        };

        var facade = CreateFacade(options, customizers);
        var response = facade.HandleException(new Exception("test"));

        response.Properties.Should().ContainKey("order");
        // Second customizer overwrites first
        response.Properties!["order"].Should().Be("second");
    }

    [Fact]
    public void Customizer_CanAccessResponseProperties()
    {
        var options = new ErrorHandlingOptions();
        var customizers = new IApiErrorResponseCustomizer[]
        {
            new CodePrefixCustomizer()
        };

        var facade = CreateFacade(options, customizers);
        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Properties.Should().ContainKey("prefixedCode");
        response.Properties!["prefixedCode"].Should().Be("ERR_INVALID_OPERATION");
    }

    [Fact]
    public void Customizer_CanAddMultipleProperties()
    {
        var options = new ErrorHandlingOptions();
        var customizers = new IApiErrorResponseCustomizer[]
        {
            new MetadataCustomizer()
        };

        var facade = CreateFacade(options, customizers);
        var response = facade.HandleException(new Exception("test"));

        response.Properties.Should().ContainKey("timestamp");
        response.Properties.Should().ContainKey("version");
        response.Properties.Should().ContainKey("environment");
    }

    private static ErrorHandlingFacade CreateFacade(
        ErrorHandlingOptions options,
        IEnumerable<IApiErrorResponseCustomizer> customizers)
    {
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

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        return new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            customizers,
            optionsWrapper,
            logger);
    }

    private class FirstCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("order", "first");
        }
    }

    private class SecondCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("order", "second");
        }
    }

    private class CodePrefixCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("prefixedCode", $"ERR_{response.Code}");
        }
    }

    private class MetadataCustomizer : IApiErrorResponseCustomizer
    {
        public void Customize(ApiErrorResponse response)
        {
            response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
            response.AddProperty("version", "1.0.0");
            response.AddProperty("environment", "test");
        }
    }
}
