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
/// Integration tests for error code strategies through the full pipeline.
/// </summary>
public class ErrorCodeStrategyIntegrationTests
{
    private ErrorHandlingFacade CreateFacade(ErrorHandlingOptions options)
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
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);
    }

    [Fact]
    public void Pipeline_AllCapsStrategy_ProducesUpperSnakeCase()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.AllCaps
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public void Pipeline_FullQualifiedNameStrategy_ProducesFullName()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.FullQualifiedName
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("System.InvalidOperationException");
    }

    [Fact]
    public void Pipeline_KebabCaseStrategy_ProducesKebabCase()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("invalid-operation");
    }

    [Fact]
    public void Pipeline_PascalCaseStrategy_ProducesPascalCase()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("InvalidOperation");
    }

    [Fact]
    public void Pipeline_DotSeparatedStrategy_ProducesDotSeparated()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("invalid.operation");
    }

    [Fact]
    public void Pipeline_KebabCaseStrategy_ArgumentException()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new ArgumentException("bad arg"));

        response.Code.Should().Be("argument");
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Pipeline_ExistingAllCapsStrategy_StillWorksUnchanged()
    {
        var options = new ErrorHandlingOptions();
        var facade = CreateFacade(options);

        var response = facade.HandleException(new KeyNotFoundException("not found"));

        response.Code.Should().Be("KEY_NOT_FOUND");
        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Pipeline_ExistingFullQualifiedStrategy_StillWorksUnchanged()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.FullQualifiedName
        };
        var facade = CreateFacade(options);

        var response = facade.HandleException(new KeyNotFoundException("not found"));

        response.Code.Should().Be("System.Collections.Generic.KeyNotFoundException");
    }

    [Fact]
    public void Pipeline_ConfiguredOverride_TakesPrecedenceOverStrategy()
    {
        var options = new ErrorHandlingOptions
        {
            DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase
        };
        options.Codes["System.ArgumentException"] = "CUSTOM_CODE";
        var facade = CreateFacade(options);

        var response = facade.HandleException(new ArgumentException("test"));

        response.Code.Should().Be("CUSTOM_CODE");
    }
}
