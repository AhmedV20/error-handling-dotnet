using System.Net;
using ErrorLens.ErrorHandling.Attributes;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration;

/// <summary>
/// Integration tests for attribute-based exception customization.
/// </summary>
public class AttributeCustomizationTests
{
    [Fact]
    public void Exception_WithResponseErrorCode_UsesCustomCode()
    {
        var facade = CreateFacade();
        var exception = new UserNotFoundException("123");

        var response = facade.HandleException(exception);

        response.Code.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public void Exception_WithResponseStatus_UsesCustomStatus()
    {
        var facade = CreateFacade();
        var exception = new UserNotFoundException("123");

        var response = facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Exception_WithResponseErrorProperty_IncludesProperty()
    {
        var facade = CreateFacade();
        var exception = new UserNotFoundException("user-123");

        var response = facade.HandleException(exception);

        response.Properties.Should().ContainKey("userId");
        response.Properties!["userId"].Should().Be("user-123");
    }

    [Fact]
    public void Exception_WithCustomPropertyName_UsesCustomName()
    {
        var facade = CreateFacade();
        var exception = new OrderNotFoundException("order-456", "store-789");

        var response = facade.HandleException(exception);

        response.Properties.Should().ContainKey("orderId");
        response.Properties.Should().ContainKey("store"); // Custom name
    }

    [Fact]
    public void Exception_WithIncludeIfNull_IncludesNullProperty()
    {
        var facade = CreateFacade();
        var exception = new OrderNotFoundException("order-456", null);

        var response = facade.HandleException(exception);

        response.Properties.Should().ContainKey("store");
        response.Properties!["store"].Should().BeNull();
    }

    [Fact]
    public void Exception_WithoutIncludeIfNull_ExcludesNullProperty()
    {
        var facade = CreateFacade();
        var exception = new UserNotFoundException(null);

        var response = facade.HandleException(exception);

        response.Properties.Should().BeNull(); // UserId is null and IncludeIfNull=false
    }

    private static ErrorHandlingFacade CreateFacade()
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

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        return new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);
    }

    [ResponseErrorCode("USER_NOT_FOUND")]
    [ResponseStatus(HttpStatusCode.NotFound)]
    private class UserNotFoundException : Exception
    {
        [ResponseErrorProperty]
        public string? UserId { get; }

        public UserNotFoundException(string? userId) : base($"User {userId} not found")
        {
            UserId = userId;
        }
    }

    [ResponseErrorCode("ORDER_NOT_FOUND")]
    [ResponseStatus(HttpStatusCode.NotFound)]
    private class OrderNotFoundException : Exception
    {
        [ResponseErrorProperty]
        public string OrderId { get; }

        [ResponseErrorProperty(Name = "store", IncludeIfNull = true)]
        public string? StoreId { get; }

        public OrderNotFoundException(string orderId, string? storeId) : base($"Order {orderId} not found")
        {
            OrderId = orderId;
            StoreId = storeId;
        }
    }
}
