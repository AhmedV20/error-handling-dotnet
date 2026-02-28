using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Integration;

/// <summary>
/// Verifies that registering FluentValidation alongside the core error handling
/// does not break handler resolution or interfere with other integrations.
/// </summary>
public class FluentValidationOpenApiTests
{
    [Fact]
    public void FluentValidationHandler_RegisteredAlongsideCore_AllHandlersResolvable()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>().ToList();

        // FluentValidationExceptionHandler should be in the handler list
        handlers.Should().Contain(h => h is FluentValidationExceptionHandler);

        // All built-in handlers should still be present
        handlers.Should().Contain(h => h.GetType().Name == "ValidationExceptionHandler");
        handlers.Should().Contain(h => h.GetType().Name == "JsonExceptionHandler");
        handlers.Should().Contain(h => h.GetType().Name == "TypeMismatchExceptionHandler");
        handlers.Should().Contain(h => h.GetType().Name == "BadRequestExceptionHandler");
    }

    [Fact]
    public void FluentValidationHandler_Order110_BetweenValidationAndJson()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>()
            .OrderBy(h => h.Order)
            .ToList();

        var fluentHandler = handlers.First(h => h is FluentValidationExceptionHandler);
        var validationHandler = handlers.First(h => h.GetType().Name == "ValidationExceptionHandler");
        var jsonHandler = handlers.First(h => h.GetType().Name == "JsonExceptionHandler");

        fluentHandler.Order.Should().Be(110);
        validationHandler.Order.Should().BeLessThan(fluentHandler.Order);
        jsonHandler.Order.Should().BeGreaterThan(fluentHandler.Order);
    }

    [Fact]
    public void FluentValidationHandler_DoesNotInterfere_WithDataAnnotationsValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>()
            .OrderBy(h => h.Order)
            .ToList();

        // DataAnnotations ValidationException should NOT be handled by FluentValidationExceptionHandler
        var dataAnnotationsException = new System.ComponentModel.DataAnnotations.ValidationException("test");
        var fluentHandler = handlers.First(h => h is FluentValidationExceptionHandler);
        fluentHandler.CanHandle(dataAnnotationsException).Should().BeFalse();

        // It SHOULD be handled by the DataAnnotations ValidationExceptionHandler
        var validationHandler = handlers.First(h => h.GetType().Name == "ValidationExceptionHandler");
        validationHandler.CanHandle(dataAnnotationsException).Should().BeTrue();
    }

    [Fact]
    public void FluentValidationRegistration_DoesNotAffect_HandlerCount()
    {
        // Registering FluentValidation should add exactly one handler
        var servicesWithout = new ServiceCollection();
        servicesWithout.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        servicesWithout.AddLogging();
        servicesWithout.AddErrorHandling();
        var countWithout = servicesWithout.BuildServiceProvider()
            .GetServices<IApiExceptionHandler>().Count();

        var servicesWith = new ServiceCollection();
        servicesWith.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        servicesWith.AddLogging();
        servicesWith.AddErrorHandling();
        servicesWith.AddErrorHandlingFluentValidation();
        var countWith = servicesWith.BuildServiceProvider()
            .GetServices<IApiExceptionHandler>().Count();

        countWith.Should().Be(countWithout + 1);
    }
}
