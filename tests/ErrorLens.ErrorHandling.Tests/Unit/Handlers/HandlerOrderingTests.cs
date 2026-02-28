using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class HandlerOrderingTests
{
    [Fact]
    public void Handlers_AreOrderedByPriority_LowerFirst()
    {
        var handler1 = CreateHandler(order: 100);
        var handler2 = CreateHandler(order: 200);
        var handler3 = CreateHandler(order: 50);

        var handlers = new[] { handler1, handler2, handler3 };
        var ordered = handlers.OrderBy(h => h.Order).ToList();

        ordered[0].Order.Should().Be(50);
        ordered[1].Order.Should().Be(100);
        ordered[2].Order.Should().Be(200);
    }

    [Fact]
    public void ValidationExceptionHandler_HasOrder100()
    {
        var options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var handler = new ValidationExceptionHandler(
            Substitute.For<IErrorCodeMapper>(),
            Substitute.For<IErrorMessageMapper>(),
            optionsWrapper);

        handler.Order.Should().Be(100);
    }

    [Fact]
    public void BadRequestExceptionHandler_HasOrder150()
    {
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(new ErrorHandlingOptions());
        var handler = new BadRequestExceptionHandler(optionsWrapper);

        handler.Order.Should().Be(150);
    }

    [Fact]
    public void AbstractApiExceptionHandler_DefaultOrder_Is1000()
    {
        var handler = new TestHandler();

        handler.Order.Should().Be(1000);
    }

    private static IApiExceptionHandler CreateHandler(int order)
    {
        var handler = Substitute.For<IApiExceptionHandler>();
        handler.Order.Returns(order);
        return handler;
    }

    private class TestHandler : AbstractApiExceptionHandler
    {
        public override bool CanHandle(Exception exception) => false;
        public override ApiErrorResponse Handle(Exception exception) => new("TEST");
    }
}
