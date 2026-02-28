using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Integration;

public class FluentValidationDITests
{
    [Fact]
    public void AddErrorHandlingFluentValidation_RegistersHandler()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>().ToList();

        handlers.Should().Contain(h => h is FluentValidationExceptionHandler);
    }

    [Fact]
    public void AddErrorHandlingFluentValidation_HandlerIsResolvable()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var handler = provider.GetServices<IApiExceptionHandler>()
            .OfType<FluentValidationExceptionHandler>()
            .FirstOrDefault();

        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandlingFluentValidation_DoubleRegistration_IsIdempotent()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();
        services.AddErrorHandlingFluentValidation(); // second call

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>()
            .OfType<FluentValidationExceptionHandler>()
            .ToList();

        handlers.Should().HaveCount(1);
    }

    [Fact]
    public void WithoutFluentValidation_FluentValidationException_FallsThrough()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        // Intentionally NOT calling AddErrorHandlingFluentValidation()

        var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>().ToList();

        var fluentException = new ValidationException(new List<ValidationFailure>
        {
            new("Email", "Required") { ErrorCode = "NotEmptyValidator" }
        });

        // No handler should be able to handle FluentValidation.ValidationException
        var matchingHandler = handlers.FirstOrDefault(h => h.CanHandle(fluentException));
        matchingHandler.Should().BeNull(
            because: "without AddErrorHandlingFluentValidation(), FluentValidation.ValidationException should fall through to the default fallback handler");
    }

    [Fact]
    public void AddErrorHandlingFluentValidation_WithOptions_ConfiguresSeverities()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation(options =>
        {
            options.IncludeSeverities.Add(Severity.Warning);
        });

        var provider = services.BuildServiceProvider();
        var handler = provider.GetServices<IApiExceptionHandler>()
            .OfType<FluentValidationExceptionHandler>()
            .FirstOrDefault();

        handler.Should().NotBeNull();

        // Verify the handler processes warnings by checking it handles a warning-severity failure
        var failures = new List<ValidationFailure>
        {
            new("Name", "Warning message") { ErrorCode = "NotEmptyValidator", Severity = Severity.Warning }
        };
        var exception = new ValidationException(failures);

        var response = handler!.Handle(exception);
        response.FieldErrors.Should().HaveCount(1);
    }
}
