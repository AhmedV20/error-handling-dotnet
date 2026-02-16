using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ErrorLens.ErrorHandling.Swashbuckle;

/// <summary>
/// Extension methods for integrating ErrorLens error response schemas with Swashbuckle.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Adds ErrorLens error response schemas to Swashbuckle Swagger documentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingSwashbuckle(this IServiceCollection services)
    {
        return services.AddErrorHandlingSwashbuckle(_ => { });
    }

    /// <summary>
    /// Adds ErrorLens error response schemas to Swashbuckle Swagger documentation with custom OpenAPI options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure OpenAPI options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingSwashbuckle(
        this IServiceCollection services,
        Action<OpenApiOptions> configure)
    {
        services.AddOptions<SwaggerGenOptions>()
            .Configure<IOptions<ErrorHandlingOptions>>((swaggerOptions, errorHandlingOptions) =>
            {
                var errorOptions = errorHandlingOptions.Value;

                // Apply any OpenAPI-specific overrides
                configure(errorOptions.OpenApi);

                swaggerOptions.OperationFilter<ErrorResponseOperationFilter>(errorOptions);
            });

        return services;
    }
}
