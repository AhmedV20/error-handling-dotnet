using System.Net;
using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.RateLimiting;

public class DefaultRateLimitResponseWriterTests
{
    private static DefaultRateLimitResponseWriter CreateWriter(
        ErrorHandlingOptions? options = null,
        IErrorMessageLocalizer? localizer = null)
    {
        var opts = options ?? new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(opts);

        var problemDetailFactory = new ProblemDetailFactory(optionsWrapper);
        var responseWriter = new ErrorResponseWriter(optionsWrapper, problemDetailFactory);
        var loc = localizer ?? new NoOpErrorMessageLocalizer();

        return new DefaultRateLimitResponseWriter(responseWriter, loc, optionsWrapper);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";
        return context;
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    // --- (1) Produces ApiErrorResponse with code RATE_LIMIT_EXCEEDED and status 429 ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_SetsStatusCode429()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_BodyContainsRateLimitExceededCode()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("\"code\":");
        body.Should().Contain("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_BodyContainsDefaultMessage()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Too many requests");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_CustomErrorCode_UsesConfigured()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { ErrorCode = "THROTTLED" }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("THROTTLED");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_CustomMessage_UsesConfigured()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { DefaultMessage = "Slow down!" }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Slow down!");
    }

    // --- (2) Sets Retry-After header when RetryAfter metadata present ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_WithRetryAfter_SetsRetryAfterHeader()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Headers["Retry-After"].ToString().Should().Be("60");
    }

    // --- (3) Sets RateLimit-* headers (separate format) ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_SeparateFormat_DoesNotSetCombinedHeader()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { UseModernHeaderFormat = false }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(30));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Headers.Should().NotContainKey("RateLimit");
    }

    // --- (4) Uses combined RateLimit header when UseModernHeaderFormat ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_ModernFormat_SetsCombinedRateLimitHeader()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { UseModernHeaderFormat = true }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(45));

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Headers["RateLimit"].ToString().Should().Contain("reset=45");
    }

    // --- (5) Includes retryAfter in body when IncludeRetryAfterInBody ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_IncludeRetryAfterInBody_AddsToResponse()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(120));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("\"retryAfter\"");
        body.Should().Contain("120");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_ExcludeRetryAfterFromBody_OmitsFromResponse()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = false }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().NotContain("\"retryAfter\"");
    }

    // --- (6) Produces ProblemDetailResponse when UseProblemDetailFormat ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_ProblemDetailFormat_SetsCorrectContentType()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_ProblemDetailFormat_BodyContainsRfc9457Fields()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("\"type\":");
        body.Should().Contain("\"title\":");
        body.Should().Contain("\"status\":");
        body.Should().Contain("\"detail\":");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_StandardFormat_SetsJsonContentType()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.ContentType.Should().Be("application/json");
    }

    // --- (7) Localizes message via IErrorMessageLocalizer ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_WithLocalizer_LocalizesMessage()
    {
        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize("RATE_LIMIT_EXCEEDED", Arg.Any<string?>())
            .Returns("Trop de requêtes. Réessayez plus tard.");

        var writer = CreateWriter(localizer: localizer);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease();

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Trop de requ");
    }

    // --- (8) Handles lease without RetryAfter metadata gracefully ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_NoRetryAfterMetadata_DoesNotSetRetryAfterHeader()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(retryAfter: null);

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.Headers.Should().NotContainKey("Retry-After");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_NoRetryAfterMetadata_StillProducesValidResponse()
    {
        var writer = CreateWriter();
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(retryAfter: null);

        await writer.WriteRateLimitResponseAsync(context, lease);

        context.Response.StatusCode.Should().Be(429);
        var body = await ReadResponseBody(context);
        body.Should().Contain("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_NoRetryAfterMetadata_OmitsRetryAfterFromBody()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(retryAfter: null);

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().NotContain("\"retryAfter\"");
    }

    // --- (9) US5: Custom RetryAfter field name ---

    [Fact]
    public async Task WriteRateLimitResponseAsync_CustomRetryAfterFieldName_UsesCustomName()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        options.JsonFieldNames.RetryAfter = "retry_after";
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(60));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("\"retry_after\"");
        body.Should().NotContain("\"retryAfter\"");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_DefaultRetryAfterFieldName_UsesRetryAfter()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(TimeSpan.FromSeconds(30));

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().Contain("\"retryAfter\"");
    }

    [Fact]
    public async Task WriteRateLimitResponseAsync_CustomRetryAfterFieldName_NoMetadata_OmitsField()
    {
        var options = new ErrorHandlingOptions
        {
            RateLimiting = new RateLimitingOptions { IncludeRetryAfterInBody = true }
        };
        options.JsonFieldNames.RetryAfter = "retry_after";
        var writer = CreateWriter(options);
        var context = CreateHttpContext();
        var lease = new TestRateLimitLease(retryAfter: null);

        await writer.WriteRateLimitResponseAsync(context, lease);

        var body = await ReadResponseBody(context);
        body.Should().NotContain("\"retry_after\"");
        body.Should().NotContain("\"retryAfter\"");
    }

    /// <summary>
    /// Concrete test double for <see cref="RateLimitLease"/> since its generic
    /// TryGetMetadata delegates to the non-generic abstract method, which NSubstitute
    /// cannot intercept correctly.
    /// </summary>
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
