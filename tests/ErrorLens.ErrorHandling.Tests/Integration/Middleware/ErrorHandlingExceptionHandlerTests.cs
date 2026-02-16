using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.Middleware;

public class ErrorHandlingExceptionHandlerTests
{
    private static (ErrorHandlingExceptionHandler handler, ServiceProvider provider) CreateHandler(
        Action<ErrorHandlingOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        if (configure != null)
            services.AddErrorHandling(configure);
        else
            services.AddErrorHandling();

        var provider = services.BuildServiceProvider();

        // ErrorHandlingExceptionHandler is registered via AddExceptionHandler<> (as IExceptionHandler),
        // so we construct it manually using the resolved dependencies.
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();
        var options = provider.GetRequiredService<IOptions<ErrorHandlingOptions>>();
        var responseWriter = provider.GetRequiredService<ErrorResponseWriter>();
        var handler = new ErrorHandlingExceptionHandler(facade, options, responseWriter);

        return (handler, provider);
    }

    [Fact]
    public async Task TryHandleAsync_Enabled_ReturnsTrue()
    {
        var (handler, provider) = CreateHandler();
        using var _ = provider;

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("test");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_Enabled_WritesErrorResponse()
    {
        var (handler, provider) = CreateHandler();
        using var _ = provider;

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("bad input");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_Disabled_ReturnsFalse()
    {
        var (handler, provider) = CreateHandler(opts => opts.Enabled = false);
        using var _ = provider;

        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("test");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_Disabled_DoesNotModifyResponse()
    {
        var (handler, provider) = CreateHandler(opts => opts.Enabled = false);
        using var _ = provider;

        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("test");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.StatusCode.Should().Be(200); // default, unmodified
    }
}
