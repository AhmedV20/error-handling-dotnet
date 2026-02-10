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
/// Integration tests for configuration-based customization.
/// </summary>
public class ConfigurationTests
{
    [Fact]
    public void Configuration_OverridesErrorCode()
    {
        var options = new ErrorHandlingOptions();
        options.Codes["System.ArgumentException"] = "INVALID_ARGUMENT";

        var facade = CreateFacade(options);
        var response = facade.HandleException(new ArgumentException("test"));

        response.Code.Should().Be("INVALID_ARGUMENT");
    }

    [Fact]
    public void Configuration_OverridesHttpStatus()
    {
        var options = new ErrorHandlingOptions();
        options.HttpStatuses["System.ArgumentException"] = HttpStatusCode.UnprocessableEntity;

        var facade = CreateFacade(options);
        var response = facade.HandleException(new ArgumentException("test"));

        response.HttpStatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Configuration_OverridesMessage()
    {
        var options = new ErrorHandlingOptions();
        options.Messages["System.ArgumentException"] = "The provided argument is invalid";

        var facade = CreateFacade(options);
        var response = facade.HandleException(new ArgumentException("original message"));

        response.Message.Should().Be("The provided argument is invalid");
    }

    [Fact]
    public void Configuration_SuperClassHierarchy_FindsBaseClassConfig()
    {
        var options = new ErrorHandlingOptions();
        options.SearchSuperClassHierarchy = true;
        options.Codes["System.SystemException"] = "SYSTEM_ERROR";

        var facade = CreateFacade(options);
        // InvalidOperationException inherits from SystemException
        var response = facade.HandleException(new InvalidOperationException("test"));

        response.Code.Should().Be("SYSTEM_ERROR");
    }

    [Fact]
    public void Configuration_FullQualifiedNameStrategy_UsesFullName()
    {
        var options = new ErrorHandlingOptions();
        options.DefaultErrorCodeStrategy = ErrorCodeStrategy.FullQualifiedName;

        var facade = CreateFacade(options);
        var response = facade.HandleException(new ArgumentException("test"));

        response.Code.Should().Be("System.ArgumentException");
    }

    [Fact]
    public void Configuration_HttpStatusInJson_IncludesStatus()
    {
        var options = new ErrorHandlingOptions();
        options.HttpStatusInJsonResponse = true;

        var facade = CreateFacade(options);
        var response = facade.HandleException(new ArgumentException("test"));

        response.Status.Should().Be(400);
    }

    [Fact]
    public void Configuration_Disabled_ThrowsException()
    {
        var options = new ErrorHandlingOptions();
        options.Enabled = false;

        var facade = CreateFacade(options);

        var act = () => facade.HandleException(new Exception("test"));

        act.Should().Throw<Exception>().WithMessage("test");
    }

    private static ErrorHandlingFacade CreateFacade(ErrorHandlingOptions options)
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
}
