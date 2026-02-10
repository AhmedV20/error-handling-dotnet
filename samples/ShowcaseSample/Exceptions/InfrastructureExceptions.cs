using System.Net;
using ErrorLens.ErrorHandling.Attributes;

namespace ShowcaseSample.Exceptions;

// --- Infrastructure Exceptions ---
// These are mapped via YAML configuration, NOT attributes.

/// <summary>
/// Thrown when a database operation times out.
/// HTTP status and error code configured via errorhandling.yml.
/// </summary>
public class DatabaseTimeoutException : Exception
{
    public string Operation { get; }

    public DatabaseTimeoutException(string operation)
        : base($"Database operation '{operation}' timed out")
    {
        Operation = operation;
    }
}

/// <summary>
/// Thrown when an external service is unavailable.
/// HTTP status and error code configured via errorhandling.yml.
/// </summary>
public class ServiceUnavailableException : Exception
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName)
        : base($"Service '{serviceName}' is currently unavailable")
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Thrown when rate limiting is exceeded.
/// HTTP status and error code configured via errorhandling.yml.
/// </summary>
public class RateLimitExceededException : Exception
{
    public int RetryAfterSeconds { get; }

    public RateLimitExceededException(int retryAfterSeconds)
        : base($"Rate limit exceeded. Retry after {retryAfterSeconds} seconds.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
