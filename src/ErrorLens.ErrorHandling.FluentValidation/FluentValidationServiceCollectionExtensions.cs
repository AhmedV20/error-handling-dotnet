using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ErrorLens.ErrorHandling.FluentValidation;

/// <summary>
/// Extension methods for registering FluentValidation error handling services.
/// </summary>
public static class FluentValidationServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentValidation exception handling to the ErrorLens error handling pipeline.
    /// Registers <see cref="FluentValidationExceptionHandler"/> to automatically catch
    /// <see cref="global::FluentValidation.ValidationException"/> and map failures to structured error responses.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingFluentValidation(this IServiceCollection services)
    {
        return services.AddErrorHandlingFluentValidation(_ => { });
    }

    /// <summary>
    /// Adds FluentValidation exception handling to the ErrorLens error handling pipeline with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="FluentValidationOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingFluentValidation(
        this IServiceCollection services,
        Action<FluentValidationOptions> configure)
    {
        services.Configure(configure);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IApiExceptionHandler, FluentValidationExceptionHandler>());

        return services;
    }
}
