using System.Net;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;

namespace FullApiSample.Filters;

/// <summary>
/// Example logging filter that suppresses logging for 404 Not Found errors.
/// This reduces log noise when resources are simply not found.
/// </summary>
public class IgnoreNotFoundFilter : ILoggingFilter
{
    public bool ShouldLog(ApiErrorResponse response, Exception exception)
    {
        // Don't log 404 errors - they're expected in normal API usage
        return response.HttpStatusCode != HttpStatusCode.NotFound;
    }
}
