using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private static (ErrorHandlingMiddleware middleware, ServiceProvider provider) CreateMiddleware(
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
        var middleware = provider.GetRequiredService<ErrorHandlingMiddleware>();
        return (middleware, provider);
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextSuccessfully()
    {
        var (middleware, provider) = CreateMiddleware();
        using var _ = provider;

        var context = new DefaultHttpContext();
        var called = false;
        RequestDelegate next = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        await middleware.InvokeAsync(context, next);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_WritesErrorResponse()
    {
        var (middleware, provider) = CreateMiddleware();
        using var _ = provider;

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("bad argument");
        RequestDelegate next = _ => throw exception;

        await middleware.InvokeAsync(context, next);

        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_Disabled_CallsNextWithoutCatching()
    {
        var (middleware, provider) = CreateMiddleware(opts => opts.Enabled = false);
        using var _ = provider;

        var context = new DefaultHttpContext();
        var called = false;
        RequestDelegate next = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        await middleware.InvokeAsync(context, next);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Disabled_ExceptionPropagates()
    {
        var (middleware, provider) = CreateMiddleware(opts => opts.Enabled = false);
        using var _ = provider;

        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("should propagate");
        RequestDelegate next = _ => throw exception;

        var act = () => middleware.InvokeAsync(context, next);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("should propagate");
    }

    [Fact]
    public async Task InvokeAsync_5xxException_ReturnsSafeMessage()
    {
        var (middleware, provider) = CreateMiddleware();
        using var _ = provider;

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        // Use plain Exception (maps to 500). InvalidOperationException maps to 400.
        var exception = new Exception("Connection string: Server=prod;Password=secret");
        RequestDelegate next = _ => throw exception;

        await middleware.InvokeAsync(context, next);

        context.Response.StatusCode.Should().Be(500);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().NotContain("secret");
        body.Should().Contain("An unexpected error occurred");
    }
}
