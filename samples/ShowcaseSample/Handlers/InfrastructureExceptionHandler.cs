using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using ShowcaseSample.Exceptions;

namespace ShowcaseSample.Handlers;

/// <summary>
/// Custom exception handler for infrastructure exceptions.
/// Demonstrates implementing IApiExceptionHandler for specialized handling.
/// </summary>
public class InfrastructureExceptionHandler : IApiExceptionHandler
{
    public int Order => 100;

    public bool CanHandle(Exception exception)
    {
        return exception is DatabaseTimeoutException
            or ServiceUnavailableException
            or RateLimitExceededException;
    }

    public ApiErrorResponse Handle(Exception exception)
    {
        return exception switch
        {
            DatabaseTimeoutException dbEx => new ApiErrorResponse(
                System.Net.HttpStatusCode.ServiceUnavailable,
                "DATABASE_TIMEOUT",
                dbEx.Message),

            ServiceUnavailableException svcEx => new ApiErrorResponse(
                System.Net.HttpStatusCode.ServiceUnavailable,
                "SERVICE_UNAVAILABLE",
                svcEx.Message),

            RateLimitExceededException rateEx => CreateRateLimitResponse(rateEx),

            _ => new ApiErrorResponse("INFRASTRUCTURE_ERROR", exception.Message)
        };
    }

    private static ApiErrorResponse CreateRateLimitResponse(RateLimitExceededException ex)
    {
        var response = new ApiErrorResponse(
            System.Net.HttpStatusCode.TooManyRequests,
            "RATE_LIMIT_EXCEEDED",
            ex.Message);
        response.AddProperty("retryAfterSeconds", ex.RetryAfterSeconds);
        return response;
    }
}
