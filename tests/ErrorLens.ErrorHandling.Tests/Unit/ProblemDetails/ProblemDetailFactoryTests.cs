using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.ProblemDetails;

public class ProblemDetailFactoryTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ProblemDetailFactory _factory;

    public ProblemDetailFactoryTests()
    {
        _options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "https://example.com/errors/"
        };
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _factory = new ProblemDetailFactory(optionsWrapper);
    }

    [Fact]
    public void CreateFromApiError_SetsTypeFromErrorCode()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "USER_NOT_FOUND", "User not found");

        var result = _factory.CreateFromApiError(apiError);

        result.Type.Should().Be("https://example.com/errors/user-not-found");
    }

    [Fact]
    public void CreateFromApiError_SetsTitleFromHttpStatus()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.NotFound, "NOT_FOUND", "Resource not found");

        var result = _factory.CreateFromApiError(apiError);

        result.Title.Should().Be("Not Found");
    }

    [Fact]
    public void CreateFromApiError_SetsStatusCode()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.UnprocessableEntity, "INVALID", "Invalid data");

        var result = _factory.CreateFromApiError(apiError);

        result.Status.Should().Be(422);
    }

    [Fact]
    public void CreateFromApiError_SetsDetailFromMessage()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "This is the detail message");

        var result = _factory.CreateFromApiError(apiError);

        result.Detail.Should().Be("This is the detail message");
    }

    [Fact]
    public void CreateFromApiError_IncludesFieldErrorsAsExtension()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Validation failed");
        apiError.AddFieldError(new ApiFieldError("EMAIL_INVALID", "email", "Invalid email", "bad-email"));

        var result = _factory.CreateFromApiError(apiError);

        result.Extensions.Should().ContainKey("fieldErrors");
    }

    [Fact]
    public void CreateFromApiError_IncludesCodeAsExtension()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "MY_ERROR_CODE", "Error message");

        var result = _factory.CreateFromApiError(apiError);

        result.Extensions.Should().ContainKey("code");
        result.Extensions["code"].Should().Be("MY_ERROR_CODE");
    }

    [Fact]
    public void CreateFromApiError_WithEmptyTypePrefix_UsesAboutBlank()
    {
        var options = new ErrorHandlingOptions { ProblemDetailTypePrefix = "" };
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);
        var factory = new ProblemDetailFactory(optionsWrapper);

        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Error");

        var result = factory.CreateFromApiError(apiError);

        result.Type.Should().Be("about:blank");
    }

    [Fact]
    public void CreateFromApiError_IncludesCustomProperties()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Error");
        apiError.AddProperty("requestId", "req-123");
        apiError.AddProperty("timestamp", "2024-01-01T00:00:00Z");

        var result = _factory.CreateFromApiError(apiError);

        result.Extensions.Should().ContainKey("requestId");
        result.Extensions["requestId"].Should().Be("req-123");
        result.Extensions.Should().ContainKey("timestamp");
    }

    // --- Kebab-case toggle tests (T036) ---

    [Fact]
    public void CreateFromApiError_KebabCaseEnabled_ConvertsErrorCodeToKebabCase()
    {
        _options.ProblemDetailConvertToKebabCase = true;
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "USER_NOT_FOUND", "User not found");

        var result = _factory.CreateFromApiError(apiError);

        result.Type.Should().Be("https://example.com/errors/user-not-found");
    }

    [Fact]
    public void CreateFromApiError_KebabCaseDisabled_PreservesOriginalErrorCode()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "https://example.com/errors/",
            ProblemDetailConvertToKebabCase = false
        };
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);
        var factory = new ProblemDetailFactory(optionsWrapper);

        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "USER_NOT_FOUND", "User not found");

        var result = factory.CreateFromApiError(apiError);

        result.Type.Should().Be("https://example.com/errors/USER_NOT_FOUND");
    }

    [Fact]
    public void CreateFromApiError_ExtensionKeysDoNotOverwriteLibraryKeys()
    {
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Error");
        apiError.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email required"));
        apiError.AddProperty("fieldErrors", "should-not-overwrite");

        var result = _factory.CreateFromApiError(apiError);

        // fieldErrors should be the list, not the string
        result.Extensions["fieldErrors"].Should().BeAssignableTo<IList<ApiFieldError>>();
    }
}
