using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Services;

/// <summary>
/// Default logging filter that allows all exceptions to be logged.
/// </summary>
public class DefaultLoggingFilter : ILoggingFilter
{
    /// <inheritdoc />
    public bool ShouldLog(ApiErrorResponse response, Exception exception)
    {
        return true;
    }
}
