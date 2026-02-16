using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration.Security;

public class InformationDisclosureTests
{
    private readonly ErrorHandlingFacade _facade;

    public InformationDisclosureTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddDebug());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddErrorHandling();

        var provider = services.BuildServiceProvider();
        _facade = provider.GetRequiredService<ErrorHandlingFacade>();
    }

    // --- 5xx information disclosure prevention ---
    // Note: Use plain Exception (maps to 500 by default) to trigger 5xx sanitization.
    // InvalidOperationException maps to 400 (BadRequest) in the default mapper.

    [Fact]
    public void Handle_DbConnectionStringException_SanitizesMessage()
    {
        // Plain Exception → 500 → message sanitized
        var exception = new Exception(
            "An error occurred using the connection string 'Server=prod-db;Password=s3cret;Database=app'");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("s3cret");
        response.Message.Should().NotContain("prod-db");
    }

    [Fact]
    public void Handle_FilePathException_SanitizesMessage()
    {
        var exception = new Exception(
            "Could not find file 'C:\\Users\\deploy\\secrets\\appsettings.Production.json'");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("C:\\Users");
    }

    [Fact]
    public void Handle_SqlQueryException_SanitizesMessage()
    {
        var exception = new Exception(
            "Invalid column name 'password_hash'. Query: SELECT * FROM users WHERE id = 1");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("SELECT");
        response.Message.Should().NotContain("password_hash");
    }

    [Fact]
    public void Handle_StackTraceInMessage_SanitizesMessage()
    {
        var exception = new Exception(
            "Error at MyApp.Internal.SecretService.ProcessPayment() in /app/src/Services/SecretService.cs:line 42");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("SecretService");
    }

    [Fact]
    public void Handle_TypeInternalsException_SanitizesMessage()
    {
        var exception = new Exception(
            "Unable to resolve service for type 'MyApp.Infrastructure.Data.SqlServerContext'");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("SqlServerContext");
    }

    [Fact]
    public void Handle_KestrelInternalMessage_SanitizesMessage()
    {
        var exception = new Exception(
            "Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpParser failed to parse request");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void Handle_NestedExceptionMessage_SanitizesMessage()
    {
        var inner = new Exception("Connection string: Server=prod;Password=secret");
        var exception = new Exception("Wrapper: " + inner.Message, inner);

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("secret");
    }

    [Fact]
    public void Handle_EnvironmentVariableLeak_SanitizesMessage()
    {
        var exception = new Exception(
            "Environment variable 'DATABASE_URL=postgres://admin:password@db:5432/prod' is invalid");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("password");
    }

    [Fact]
    public void Handle_AssemblyNamespaceInMessage_SanitizesMessage()
    {
        var exception = new Exception(
            "System.Reflection.TargetInvocationException: MyApp.Domain.Internal.PaymentProcessor threw an exception");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("PaymentProcessor");
    }

    [Fact]
    public void Handle_Custom5xxWithSensitiveMessage_SanitizesMessage()
    {
        var exception = new HttpRequestException(
            "Response status code does not indicate success: 502 (Bad Gateway). Connection to upstream server 10.0.0.5:8080 failed");

        var response = _facade.HandleException(exception);

        response.Message.Should().Be("An unexpected error occurred");
        response.Message.Should().NotContain("10.0.0.5");
    }

    // --- 4xx preservation regression tests (FR-002) ---

    [Fact]
    public void Handle_ArgumentException_PreservesOriginalMessage()
    {
        var exception = new ArgumentException("The 'email' field must be a valid email address");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Message.Should().Be("The 'email' field must be a valid email address");
    }

    [Fact]
    public void Handle_KeyNotFoundException_PreservesOriginalMessage()
    {
        var exception = new KeyNotFoundException("User with ID 'abc-123' was not found");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Message.Should().Be("User with ID 'abc-123' was not found");
    }

    [Fact]
    public void Handle_UnauthorizedAccessException_PreservesOriginalMessage()
    {
        var exception = new UnauthorizedAccessException("You do not have permission to access this resource");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Message.Should().Be("You do not have permission to access this resource");
    }

    [Fact]
    public void Handle_TimeoutException_Preserves4xxMessage()
    {
        var exception = new TimeoutException("The operation timed out after 30 seconds");

        var response = _facade.HandleException(exception);

        response.HttpStatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        response.Message.Should().Be("The operation timed out after 30 seconds");
    }
}
