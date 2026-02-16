using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.RateLimiting;

public class RateLimitResponseTests
{
    // --- DI registration tests ---

    [Fact]
    public void AddErrorHandling_RegistersIRateLimitResponseWriter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.AddErrorHandling();

        var provider = services.BuildServiceProvider();

        var writer = provider.GetService<IRateLimitResponseWriter>();
        writer.Should().NotBeNull();
        writer.Should().BeOfType<DefaultRateLimitResponseWriter>();
    }

    [Fact]
    public void AddErrorHandling_CustomWriter_TakesPrecedence()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

        var customWriter = Substitute.For<IRateLimitResponseWriter>();
        services.AddSingleton(customWriter);
        services.AddErrorHandling();

        var provider = services.BuildServiceProvider();

        var writer = provider.GetService<IRateLimitResponseWriter>();
        writer.Should().BeSameAs(customWriter);
    }

    // --- Full pipeline test ---

    [Fact]
    public async Task FullPipeline_WriterProducesStructuredResponse()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.AddErrorHandling();

        var provider = services.BuildServiceProvider();
        var writer = provider.GetRequiredService<IRateLimitResponseWriter>();

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/data";

        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(30));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers["Retry-After"].ToString().Should().Be("30");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("RATE_LIMIT_EXCEEDED");
        body.Should().Contain("Too many requests");
    }

    [Fact]
    public async Task FullPipeline_WithLocalization_LocalizesMessage()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize("RATE_LIMIT_EXCEEDED", Arg.Any<string?>())
            .Returns("Localized rate limit message");
        services.AddSingleton(localizer);

        services.AddErrorHandling();

        var provider = services.BuildServiceProvider();
        var writer = provider.GetRequiredService<IRateLimitResponseWriter>();

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("Localized rate limit message");
    }

    private sealed class TestRateLimitLease : RateLimitLease
    {
        private readonly TimeSpan? _retryAfter;

        public TestRateLimitLease(TimeSpan? retryAfter = null)
        {
            _retryAfter = retryAfter;
        }

        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames =>
            _retryAfter.HasValue ? ["RETRY_AFTER"] : [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (metadataName == "RETRY_AFTER" && _retryAfter.HasValue)
            {
                metadata = _retryAfter.Value;
                return true;
            }

            metadata = null;
            return false;
        }
    }
}
