#if NET7_0_OR_GREATER
using System.Net;
using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.RateLimiting;

/// <summary>
/// Default implementation of <see cref="IRateLimitResponseWriter"/> that produces
/// structured ErrorLens error responses for rate limit rejections.
/// </summary>
public class DefaultRateLimitResponseWriter : IRateLimitResponseWriter
{
    private readonly ErrorResponseWriter _responseWriter;
    private readonly IErrorMessageLocalizer _localizer;
    private readonly ErrorHandlingOptions _options;

    public DefaultRateLimitResponseWriter(
        ErrorResponseWriter responseWriter,
        IErrorMessageLocalizer localizer,
        IOptions<ErrorHandlingOptions> options)
    {
        _responseWriter = responseWriter;
        _localizer = localizer;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task WriteRateLimitResponseAsync(
        HttpContext context,
        RateLimitLease lease,
        CancellationToken cancellationToken = default)
    {
        var rateLimitOptions = _options.RateLimiting;

        // Localize the message
        var message = _localizer.Localize(rateLimitOptions.ErrorCode, rateLimitOptions.DefaultMessage)
                      ?? rateLimitOptions.DefaultMessage;

        // Build the error response
        var response = new ApiErrorResponse(HttpStatusCode.TooManyRequests, rateLimitOptions.ErrorCode, message);

        // Extract RetryAfter metadata from the lease
        TimeSpan? retryAfter = null;
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
        {
            retryAfter = retryAfterValue;
            // Round up to avoid truncating sub-second values to 0 (which means "retry immediately")
            var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfterValue.TotalSeconds));

            // Set Retry-After header (always, when metadata is available)
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

            // Set RateLimit headers
            if (rateLimitOptions.UseModernHeaderFormat)
            {
                // Combined format (IETF draft): RateLimit: remaining=0, reset=Y
                context.Response.Headers["RateLimit"] = $"remaining=0, reset={retryAfterSeconds}";
            }

            // Include retryAfter in response body if configured
            if (rateLimitOptions.IncludeRetryAfterInBody)
            {
                response.AddProperty("retryAfter", retryAfterSeconds);
            }
        }

        // Delegate body writing to the shared ErrorResponseWriter
        await _responseWriter.WriteResponseAsync(context, response, cancellationToken);
    }
}
#endif
