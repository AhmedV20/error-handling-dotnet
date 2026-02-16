namespace ErrorLens.ErrorHandling.RateLimiting;

/// <summary>
/// Configuration options for rate limiting error responses.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Error code for rate limit responses. Default: "RATE_LIMIT_EXCEEDED"
    /// </summary>
    public string ErrorCode { get; set; } = "RATE_LIMIT_EXCEEDED";

    /// <summary>
    /// Default message for rate limit responses.
    /// </summary>
    public string DefaultMessage { get; set; } = "Too many requests. Please try again later.";

    /// <summary>
    /// Include retryAfter property in the JSON response body. Default: true
    /// </summary>
    public bool IncludeRetryAfterInBody { get; set; } = true;

    /// <summary>
    /// Use the combined RateLimit header format (IETF draft) instead of separate
    /// RateLimit-Limit, RateLimit-Remaining, RateLimit-Reset headers. Default: false
    /// </summary>
    public bool UseModernHeaderFormat { get; set; } = false;
}
