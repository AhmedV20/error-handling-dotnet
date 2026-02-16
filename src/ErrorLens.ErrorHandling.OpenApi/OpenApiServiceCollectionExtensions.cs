using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.OpenApi;

/// <summary>
/// Extension methods for integrating ErrorLens error response schemas with .NET 9+ OpenAPI.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds ErrorLens error response schemas to .NET 9+ OpenAPI documentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingOpenApi(this IServiceCollection services)
    {
        return services.AddErrorHandlingOpenApi(_ => { });
    }

    /// <summary>
    /// Adds ErrorLens error response schemas to .NET 9+ OpenAPI documentation with custom OpenAPI options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure OpenAPI options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingOpenApi(
        this IServiceCollection services,
        Action<OpenApiOptions> configure)
    {
        services.AddOptions<Microsoft.AspNetCore.OpenApi.OpenApiOptions>()
            .Configure<IOptions<ErrorHandlingOptions>>((openApiOptions, errorHandlingOptions) =>
            {
                var errorOptions = errorHandlingOptions.Value;

                // Apply any OpenAPI-specific overrides
                configure(errorOptions.OpenApi);

                openApiOptions.AddOperationTransformer(new ErrorResponseOperationTransformer(errorOptions));
            });

        return services;
    }
}
