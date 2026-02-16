using System.Net;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class AggregateExceptionHandlerTests
{
    [Fact]
    public void Order_Is50()
    {
        var handler = CreateHandler();
        handler.Order.Should().Be(50);
    }

    [Fact]
    public void CanHandle_AggregateException_ReturnsTrue()
    {
        var handler = CreateHandler();
        handler.CanHandle(new AggregateException()).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherException_ReturnsFalse()
    {
        var handler = CreateHandler();
        handler.CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Handle_SingleInnerException_UnwrapsAndDispatches()
    {
        var innerHandler = Substitute.For<IApiExceptionHandler>();
        innerHandler.Order.Returns(100);
        innerHandler.CanHandle(Arg.Is<Exception>(e => e is InvalidOperationException)).Returns(true);
        innerHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.BadRequest, "INVALID_OPERATION", "Test"));

        var fallback = Substitute.For<IFallbackApiExceptionHandler>();

        var handler = CreateHandler(new[] { innerHandler }, fallback);
        var aggregate = new AggregateException(new InvalidOperationException("inner"));

        var response = handler.Handle(aggregate);

        response.Code.Should().Be("INVALID_OPERATION");
        innerHandler.Received(1).Handle(Arg.Any<Exception>());
        fallback.DidNotReceive().Handle(Arg.Any<Exception>());
    }

    [Fact]
    public void Handle_SingleInnerException_NoMatchingHandler_UsesFallback()
    {
        var fallback = Substitute.For<IFallbackApiExceptionHandler>();
        fallback.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "FALLBACK", "Fallback"));

        var handler = CreateHandler(Array.Empty<IApiExceptionHandler>(), fallback);
        var aggregate = new AggregateException(new Exception("no handler"));

        var response = handler.Handle(aggregate);

        response.Code.Should().Be("FALLBACK");
        fallback.Received(1).Handle(Arg.Any<Exception>());
    }

    [Fact]
    public void Handle_MultipleInnerExceptions_DelegatesToFallback()
    {
        var fallback = Substitute.For<IFallbackApiExceptionHandler>();
        fallback.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "AGGREGATE", "Multiple errors"));

        var handler = CreateHandler(Array.Empty<IApiExceptionHandler>(), fallback);
        var aggregate = new AggregateException(
            new Exception("first"),
            new Exception("second"));

        var response = handler.Handle(aggregate);

        response.Code.Should().Be("AGGREGATE");
        fallback.Received(1).Handle(aggregate);
    }

    [Fact]
    public void Handle_WrongExceptionType_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();

        var act = () => handler.Handle(new InvalidOperationException());

        act.Should().Throw<InvalidOperationException>().WithMessage("*CanHandle*");
    }

    [Fact]
    public void Handle_NestedAggregate_FlattensBeforeDispatching()
    {
        var innerHandler = Substitute.For<IApiExceptionHandler>();
        innerHandler.Order.Returns(100);
        innerHandler.CanHandle(Arg.Is<Exception>(e => e is ArgumentException)).Returns(true);
        innerHandler.Handle(Arg.Any<Exception>())
            .Returns(new ApiErrorResponse(HttpStatusCode.BadRequest, "ARGUMENT", "Bad arg"));

        var fallback = Substitute.For<IFallbackApiExceptionHandler>();

        var handler = CreateHandler(new[] { innerHandler }, fallback);
        // Nested: AggregateException(AggregateException(ArgumentException))
        var nested = new AggregateException(new AggregateException(new ArgumentException("nested")));

        var response = handler.Handle(nested);

        response.Code.Should().Be("ARGUMENT");
    }

    private static AggregateExceptionHandler CreateHandler(
        IApiExceptionHandler[]? handlers = null,
        IFallbackApiExceptionHandler? fallback = null)
    {
        var services = new ServiceCollection();

        foreach (var h in handlers ?? Array.Empty<IApiExceptionHandler>())
        {
            services.AddSingleton(h);
        }

        if (fallback == null)
        {
            fallback = Substitute.For<IFallbackApiExceptionHandler>();
            fallback.Handle(Arg.Any<Exception>())
                .Returns(new ApiErrorResponse(HttpStatusCode.InternalServerError, "DEFAULT", "Default"));
        }
        services.AddSingleton(fallback);

        // Register the handlers as IApiExceptionHandler
        foreach (var h in handlers ?? Array.Empty<IApiExceptionHandler>())
        {
            services.AddSingleton<IApiExceptionHandler>(h);
        }

        var provider = services.BuildServiceProvider();
        return new AggregateExceptionHandler(provider);
    }
}
