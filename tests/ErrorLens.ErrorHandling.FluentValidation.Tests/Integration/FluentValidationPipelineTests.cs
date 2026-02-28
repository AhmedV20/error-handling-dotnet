using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Integration;

public class FluentValidationPipelineTests
{
    [Fact]
    public void FluentValidationException_FlowsThroughFacade_Correctly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

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

        response.Code.Should().Be(DefaultErrorCodes.ValidationFailed);
        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Code.Should().Be(DefaultErrorCodes.RequiredNotEmpty);
        response.FieldErrors[0].Property.Should().Be("email");
    }

    [Fact]
    public void CustomErrorCodeMapper_IsRespected_InPipeline()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling(options =>
        {
            options.Codes["Email.NotEmptyValidator"] = "CUSTOM_EMAIL_REQUIRED";
        });
        services.AddErrorHandlingFluentValidation();

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
        response.FieldErrors![0].Code.Should().Be("CUSTOM_EMAIL_REQUIRED");
    }

    [Fact]
    public void CustomErrorMessageMapper_IsRespected_InPipeline()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling(options =>
        {
            options.Messages["Email.NotEmptyValidator"] = "Email is required, please provide one.";
        });
        services.AddErrorHandlingFluentValidation();

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
        response.FieldErrors![0].Message.Should().Be("Email is required, please provide one.");
    }

    [Fact]
    public void MultipleFieldErrors_AllMappedCorrectly_InPipeline()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();

        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' must not be empty.") { ErrorCode = "NotEmptyValidator", AttemptedValue = "" },
            new("Age", "'Age' must be greater than '0'.") { ErrorCode = "GreaterThanValidator", AttemptedValue = -5 },
            new("Name", "'Name' is required.") { ErrorCode = "NotNullValidator", AttemptedValue = null }
        };
        var exception = new ValidationException(failures);

        var response = facade.HandleException(exception);

        response.FieldErrors.Should().HaveCount(3);
        response.FieldErrors![0].Code.Should().Be(DefaultErrorCodes.RequiredNotEmpty);
        response.FieldErrors[1].Code.Should().Be(DefaultErrorCodes.InvalidMin);
        response.FieldErrors[2].Code.Should().Be(DefaultErrorCodes.RequiredNotNull);
    }

    [Fact]
    public void GlobalAndFieldErrors_MixedCorrectly_InPipeline()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddErrorHandling();
        services.AddErrorHandlingFluentValidation();

        var provider = services.BuildServiceProvider();
        var facade = provider.GetRequiredService<ErrorHandlingFacade>();

        var failures = new List<ValidationFailure>
        {
            new("Email", "Field error") { ErrorCode = "NotEmptyValidator" },
            new("", "Global error") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new ValidationException(failures);

        var response = facade.HandleException(exception);

        response.FieldErrors.Should().HaveCount(1);
        response.GlobalErrors.Should().HaveCount(1);
    }
}
