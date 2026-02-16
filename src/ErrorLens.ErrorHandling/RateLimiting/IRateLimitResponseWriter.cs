#if NET7_0_OR_GREATER
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace ErrorLens.ErrorHandling.RateLimiting;

/// <summary>
/// Writes a structured ErrorLens-formatted response when a request is rejected by the rate limiter.
/// Wire this into <c>RateLimiterOptions.OnRejected</c> to produce consistent error responses.
/// </summary>
public interface IRateLimitResponseWriter
{
    /// <summary>
    /// Writes a rate limit exceeded response to the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="lease">The rate limit lease containing metadata.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task WriteRateLimitResponseAsync(
        HttpContext context,
        RateLimitLease lease,
        CancellationToken cancellationToken = default);
}
#endif
