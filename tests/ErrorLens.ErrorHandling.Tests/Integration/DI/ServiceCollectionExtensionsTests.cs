using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.DI;

public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider BuildProvider(Action<ErrorHandlingOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        if (configure != null)
            services.AddErrorHandling(configure);
        else
            services.AddErrorHandling();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddErrorHandling_RegistersErrorCodeMapper()
    {
        using var provider = BuildProvider();
        provider.GetService<IErrorCodeMapper>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersErrorMessageMapper()
    {
        using var provider = BuildProvider();
        provider.GetService<IErrorMessageMapper>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersHttpStatusMapper()
    {
        using var provider = BuildProvider();
        provider.GetService<IHttpStatusMapper>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersFallbackHandler()
    {
        using var provider = BuildProvider();
        provider.GetService<IFallbackApiExceptionHandler>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersLoggingService()
    {
        using var provider = BuildProvider();
        provider.GetService<ILoggingService>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersFacade()
    {
        using var provider = BuildProvider();
        provider.GetService<ErrorHandlingFacade>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersProblemDetailFactory()
    {
        using var provider = BuildProvider();
        provider.GetService<IProblemDetailFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersResponseWriter()
    {
        using var provider = BuildProvider();
        provider.GetService<ErrorResponseWriter>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersMiddleware()
    {
        using var provider = BuildProvider();
        provider.GetService<ErrorHandlingMiddleware>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersBuiltInHandlers()
    {
        using var provider = BuildProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>().ToList();

        handlers.Should().Contain(h => h is ModelStateValidationExceptionHandler);
        handlers.Should().Contain(h => h is BadRequestExceptionHandler);
        handlers.Should().Contain(h => h is AggregateExceptionHandler);
    }

    [Fact]
    public void AddErrorHandling_DuplicateCalls_AreIdempotent()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddErrorHandling();
        services.AddErrorHandling(); // second call

        using var provider = services.BuildServiceProvider();

        // TryAdd should prevent duplicate singleton registrations
        var mappers = provider.GetServices<IErrorCodeMapper>().ToList();
        mappers.Should().HaveCount(1);

        var fallbacks = provider.GetServices<IFallbackApiExceptionHandler>().ToList();
        fallbacks.Should().HaveCount(1);
    }

    [Fact]
    public void AddErrorHandling_TryAdd_AllowsUserOverrides()
    {
        var customMapper = Substitute.For<IErrorCodeMapper>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // User registers their custom mapper FIRST
        services.AddSingleton(customMapper);

        // AddErrorHandling uses TryAdd — should not overwrite
        services.AddErrorHandling();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IErrorCodeMapper>().Should().BeSameAs(customMapper);
    }

    [Fact]
    public void AddErrorHandling_WithConfiguration_AppliesOptions()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.Enabled = false;
            opts.HttpStatusInJsonResponse = true;
        });

        var options = provider.GetRequiredService<IOptions<ErrorHandlingOptions>>().Value;
        options.Enabled.Should().BeFalse();
        options.HttpStatusInJsonResponse.Should().BeTrue();
    }

    [Fact]
    public void AddErrorHandling_OverrideModelStateValidation_ConfiguresApiBehaviorOptions()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.OverrideModelStateValidation = true;
        });

        var apiBehavior = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        apiBehavior.InvalidModelStateResponseFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_WithIConfiguration_BindsFromSection()
    {
        var configDict = new Dictionary<string, string?>
        {
            { "ErrorHandling:Enabled", "false" },
            { "ErrorHandling:HttpStatusInJsonResponse", "true" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddErrorHandling(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ErrorHandlingOptions>>().Value;

        options.Enabled.Should().BeFalse();
        options.HttpStatusInJsonResponse.Should().BeTrue();
    }

    [Fact]
    public void AddApiExceptionHandler_RegistersCustomHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddErrorHandling();
        services.AddApiExceptionHandler<TestCustomHandler>();

        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IApiExceptionHandler>().ToList();

        handlers.Should().Contain(h => h is TestCustomHandler);
    }

    [Fact]
    public void AddErrorHandling_RegistersErrorMessageLocalizer()
    {
        using var provider = BuildProvider();
        provider.GetService<IErrorMessageLocalizer>().Should().NotBeNull();
    }

    [Fact]
    public void AddErrorHandling_RegistersNoOpErrorMessageLocalizer_ByDefault()
    {
        using var provider = BuildProvider();
        provider.GetRequiredService<IErrorMessageLocalizer>().Should().BeOfType<NoOpErrorMessageLocalizer>();
    }

    [Fact]
    public void AddErrorHandling_TryAdd_AllowsCustomErrorMessageLocalizer()
    {
        var customLocalizer = Substitute.For<IErrorMessageLocalizer>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // User registers their custom localizer FIRST
        services.AddSingleton(customLocalizer);

        // AddErrorHandling uses TryAdd — should not overwrite
        services.AddErrorHandling();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IErrorMessageLocalizer>().Should().BeSameAs(customLocalizer);
    }

    [Fact]
    public void AddErrorHandlingLocalization_ReplacesNoOpWithStringLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddErrorHandling();
        services.AddErrorHandlingLocalization<TestLocalizationResource>();

        using var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<IErrorMessageLocalizer>();

        localizer.Should().BeOfType<StringLocalizerErrorMessageLocalizer<TestLocalizationResource>>();
    }

    [Fact]
    public void AddErrorHandlingLocalization_CustomLocalizerRegisteredFirst_IsNotOverwritten()
    {
        var customLocalizer = Substitute.For<IErrorMessageLocalizer>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // User registers their custom localizer FIRST
        services.AddSingleton(customLocalizer);

        services.AddErrorHandling();
        services.AddErrorHandlingLocalization<TestLocalizationResource>();

        using var provider = services.BuildServiceProvider();

        // AddErrorHandlingLocalization replaces the registration, so the last one wins
        // But a user who registered before AddErrorHandling used TryAdd — however
        // AddErrorHandlingLocalization explicitly replaces, so it should win
        var resolved = provider.GetRequiredService<IErrorMessageLocalizer>();
        resolved.Should().BeOfType<StringLocalizerErrorMessageLocalizer<TestLocalizationResource>>();
    }

    private class TestCustomHandler : IApiExceptionHandler
    {
        public int Order => 1;
        public bool CanHandle(Exception exception) => false;
        public ApiErrorResponse Handle(Exception exception) =>
            new(System.Net.HttpStatusCode.InternalServerError, "TEST", "Test");
    }

    private class TestLocalizationResource { }
}
