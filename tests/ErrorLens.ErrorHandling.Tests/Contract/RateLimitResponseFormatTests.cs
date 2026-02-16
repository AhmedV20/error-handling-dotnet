using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Contract;

public class RateLimitResponseFormatTests
{
    private static DefaultRateLimitResponseWriter CreateWriter(ErrorHandlingOptions? options = null)
    {
        var opts = options ?? new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(opts);

        var problemDetailFactory = new ProblemDetailFactory(optionsWrapper);
        var responseWriter = new ErrorResponseWriter(optionsWrapper, problemDetailFactory);
        var localizer = new NoOpErrorMessageLocalizer();

        return new DefaultRateLimitResponseWriter(responseWriter, localizer, optionsWrapper);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/resource";
        return context;
    }

    private static async Task<JsonDocument> GetResponseJson(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    // --- Standard format contract ---

    [Fact]
    public async Task StandardFormat_HasRequiredCodeField()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("code").GetString().Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task StandardFormat_HasMessageField()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("message").GetString()
            .Should().Be("Too many requests. Please try again later.");
    }

    [Fact]
    public async Task StandardFormat_WithRetryAfterInBody_HasRetryAfterField()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("retryAfter").GetInt32().Should().Be(60);
    }

    [Fact]
    public async Task StandardFormat_IsValidJson()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(30));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var act = async () => await JsonDocument.ParseAsync(context.Response.Body);
        await act.Should().NotThrowAsync();
    }

    // --- Problem Details format contract ---

    [Fact]
    public async Task ProblemDetailFormat_HasTypeField()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("type").GetString().Should().Contain("rate-limit-exceeded");
    }

    [Fact]
    public async Task ProblemDetailFormat_HasTitleField()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("title").GetString().Should().Be("Too Many Requests");
    }

    [Fact]
    public async Task ProblemDetailFormat_HasStatusField429()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("status").GetInt32().Should().Be(429);
    }

    [Fact]
    public async Task ProblemDetailFormat_HasDetailField()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("detail").GetString()
            .Should().Be("Too many requests. Please try again later.");
    }

    [Fact]
    public async Task ProblemDetailFormat_HasInstanceField()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("instance").GetString().Should().Be("/api/resource");
    }

    [Fact]
    public async Task ProblemDetailFormat_HasCodeExtension()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var json = await GetResponseJson(context);
        json.RootElement.GetProperty("code").GetString().Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task ProblemDetailFormat_IsValidJson()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(30));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var act = async () => await JsonDocument.ParseAsync(context.Response.Body);
        await act.Should().NotThrowAsync();
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
