using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Localization;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Integration;

public class FluentValidationLocalizationTests
{
    [Fact]
    public void Localization_AppliedToFluentValidationErrors()
    {
        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(callInfo => callInfo.ArgAt<string?>(1));
        localizer.LocalizeFieldError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("Localized field error message");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();
        services.Replace(ServiceDescriptor.Singleton(localizer));

        var provider = services.BuildServiceProvider();
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();

        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.")
            {
                ErrorCode = "NotEmptyValidator",
                AttemptedValue = ""
            }
        };
        var exception = new ValidationException(failures);

        var response = facade.HandleException(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Message.Should().Be("Localized field error message");

        localizer.Received().LocalizeFieldError(
            Arg.Any<string>(),
            Arg.Is<string>(s => s == "email"),
            Arg.Any<string>());
    }

    [Fact]
    public void Localization_AppliedToGlobalErrors()
    {
        var localizer = Substitute.For<IErrorMessageLocalizer>();
        localizer.Localize(DefaultErrorCodes.ValidationFailed, Arg.Any<string?>())
            .Returns("Localized validation failed");
        localizer.Localize(Arg.Is<string>(s => s != DefaultErrorCodes.ValidationFailed), Arg.Any<string?>())
            .Returns("Localized global error");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();
        services.Replace(ServiceDescriptor.Singleton(localizer));

        var provider = services.BuildServiceProvider();
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();

        var failures = new List<ValidationFailure>
        {
            new("", "Global validation error") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = facade.HandleException(exception);

        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Message.Should().Be("Localized global error");
    }

    [Fact]
    public void NoLocalization_OriginalMessagesPreserved()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();
        // Not replacing localizer — default NoOpErrorMessageLocalizer is used

        var provider = services.BuildServiceProvider();
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();

        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.")
            {
                ErrorCode = "NotEmptyValidator",
                AttemptedValue = ""
            }
        };
        var exception = new ValidationException(failures);

        var response = facade.HandleException(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Message.Should().Be("'Email' must not be empty.");
    }
}
