using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.RateLimiting;
using ErrorLens.ErrorHandling.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Extensions;

/// <summary>
/// Extension methods for configuring error handling services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds error handling services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        return services.AddErrorHandling(_ => { });
    }

    /// <summary>
    /// Adds error handling services to the service collection with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandling(
        this IServiceCollection services,
        Action<ErrorHandlingOptions> configure)
    {
        // Configure options
        services.AddOptions<ErrorHandlingOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(ErrorHandlingOptions.SectionName).Bind(options);
            })
            .Configure(configure);

        return RegisterCoreServices(services);
    }

    /// <summary>
    /// Adds error handling services to the service collection with IConfiguration binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ErrorHandlingOptions>(
            configuration.GetSection(ErrorHandlingOptions.SectionName));

        return RegisterCoreServices(services);
    }

    private static IServiceCollection RegisterCoreServices(IServiceCollection services)
    {
        // Register options validation
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<ErrorHandlingOptions>, ErrorHandlingOptionsValidator>());

        // Register mappers
        services.TryAddSingleton<IErrorCodeMapper, ErrorCodeMapper>();
        services.TryAddSingleton<IErrorMessageMapper, ErrorMessageMapper>();
        services.TryAddSingleton<IHttpStatusMapper, HttpStatusMapper>();

        // Register logging service
        services.TryAddSingleton<ILoggingService, LoggingService>();

        // Register localization (no-op by default — replaced when user calls AddErrorHandlingLocalization)
        services.TryAddSingleton<IErrorMessageLocalizer, NoOpErrorMessageLocalizer>();

        // Register fallback handler
        services.TryAddSingleton<IFallbackApiExceptionHandler, DefaultFallbackHandler>();

        // Register built-in exception handlers (idempotent via TryAddEnumerable)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, ModelStateValidationExceptionHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, ValidationExceptionHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, JsonExceptionHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, TypeMismatchExceptionHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, BadRequestExceptionHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, AggregateExceptionHandler>());

        // Conditionally intercept [ApiController] automatic model validation
        // Only when OverrideModelStateValidation is true
        services.AddOptions<ApiBehaviorOptions>()
            .Configure<IOptions<ErrorHandlingOptions>>((apiBehavior, errorOptions) =>
            {
                if (errorOptions.Value.OverrideModelStateValidation)
                {
                    apiBehavior.InvalidModelStateResponseFactory = context =>
                    {
                        throw new ModelStateValidationException(context.ModelState);
                    };
                }
            });

        // Register facade
        services.TryAddSingleton<ErrorHandlingFacade>();

        // Register Problem Details factory
        services.TryAddSingleton<IProblemDetailFactory, ProblemDetailFactory>();

        // Register shared response writer (caches JsonSerializerOptions)
        services.TryAddSingleton<ErrorResponseWriter>();

        // Register middleware
        services.TryAddSingleton<ErrorHandlingMiddleware>();

#if NET7_0_OR_GREATER
        // Register rate limit response writer for .NET 7+
        services.TryAddSingleton<IRateLimitResponseWriter, DefaultRateLimitResponseWriter>();
#endif

#if NET8_0_OR_GREATER
        // Register IExceptionHandler for .NET 8+
        services.AddExceptionHandler<ErrorHandlingExceptionHandler>();
#endif

        return services;
    }

    /// <summary>
    /// Adds a custom exception handler for the ErrorLens error handling pipeline.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiExceptionHandler<THandler>(this IServiceCollection services)
        where THandler : class, IApiExceptionHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiExceptionHandler, THandler>());
        return services;
    }

    /// <summary>
    /// Adds a custom error response customizer.
    /// </summary>
    /// <typeparam name="TCustomizer">The customizer type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorResponseCustomizer<TCustomizer>(this IServiceCollection services)
        where TCustomizer : class, IApiErrorResponseCustomizer
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiErrorResponseCustomizer, TCustomizer>());
        return services;
    }

    /// <summary>
    /// Opts into localization for error messages using <see cref="Microsoft.Extensions.Localization.IStringLocalizer{TResource}"/>.
    /// Replaces the default <see cref="NoOpErrorMessageLocalizer"/> with <see cref="StringLocalizerErrorMessageLocalizer{TResource}"/>.
    /// </summary>
    /// <typeparam name="TResource">The resource type for <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/> resolution.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorHandlingLocalization<TResource>(this IServiceCollection services)
        where TResource : class
    {
        services.AddLocalization();

        // Replace the default NoOp registration with the StringLocalizer bridge
        services.Replace(ServiceDescriptor.Singleton<IErrorMessageLocalizer, StringLocalizerErrorMessageLocalizer<TResource>>());

        return services;
    }
}
