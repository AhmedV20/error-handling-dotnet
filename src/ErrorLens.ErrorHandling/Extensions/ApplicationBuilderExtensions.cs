using ErrorLens.ErrorHandling.Integration;
using Microsoft.AspNetCore.Builder;

namespace ErrorLens.ErrorHandling.Extensions;

/// <summary>
/// Extension methods for configuring error handling middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the error handling middleware to the application pipeline.
    /// This should be called early in the pipeline to catch all exceptions.
    /// Required for .NET 6/7. Optional for .NET 8+ (IExceptionHandler is used by default).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
