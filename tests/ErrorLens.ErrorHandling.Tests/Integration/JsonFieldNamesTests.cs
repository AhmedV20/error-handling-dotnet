using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Mappers;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Serialization;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration;

/// <summary>
/// Integration tests verifying custom JSON field names flow end-to-end
/// from configuration through exception handling to serialized JSON output.
/// </summary>
public class JsonFieldNamesTests
{
    private static (ErrorHandlingFacade facade, ErrorHandlingOptions options) CreateFacade(
        Action<ErrorHandlingOptions>? configure = null)
    {
        var options = new ErrorHandlingOptions();
        configure?.Invoke(options);

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper,
            errorMessageMapper,
            httpStatusMapper,
            optionsWrapper);

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();

        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            Enumerable.Empty<IApiErrorResponseCustomizer>(),
            optionsWrapper,
            logger);

        return (facade, options);
    }

    private static JsonSerializerOptions CreateJsonOptions(JsonFieldNamesOptions fieldNames)
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ApiErrorResponseConverter(fieldNames) }
        };
    }

    [Fact]
    public void EndToEnd_DefaultFieldNames_ProducesStandardJson()
    {
        var (facade, options) = CreateFacade();
        var exception = new InvalidOperationException("Something went wrong");

        var response = facade.HandleException(exception);
        var jsonOptions = CreateJsonOptions(options.JsonFieldNames);
        var json = JsonSerializer.Serialize(response, jsonOptions);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("INVALID_OPERATION");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Something went wrong");
        doc.RootElement.TryGetProperty("status", out _).Should().BeFalse();
    }

    [Fact]
    public void EndToEnd_CustomFieldNames_CodeAndMessage()
    {
        var (facade, options) = CreateFacade(o =>
        {
            o.JsonFieldNames.Code = "type";
            o.JsonFieldNames.Message = "detail";
        });

        var exception = new InvalidOperationException("Something went wrong");
        var response = facade.HandleException(exception);
        var jsonOptions = CreateJsonOptions(options.JsonFieldNames);
        var json = JsonSerializer.Serialize(response, jsonOptions);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("type").GetString().Should().Be("INVALID_OPERATION");
        doc.RootElement.GetProperty("detail").GetString().Should().Be("Something went wrong");

        // Original names must not exist
        doc.RootElement.TryGetProperty("code", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
    }

    [Fact]
    public void EndToEnd_CustomFieldNames_WithStatus()
    {
        var (facade, options) = CreateFacade(o =>
        {
            o.HttpStatusInJsonResponse = true;
            o.JsonFieldNames.Status = "httpStatus";
        });

        var exception = new InvalidOperationException("Test");
        var response = facade.HandleException(exception);
        var jsonOptions = CreateJsonOptions(options.JsonFieldNames);
        var json = JsonSerializer.Serialize(response, jsonOptions);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("httpStatus").GetInt32().Should().Be(400);
        doc.RootElement.TryGetProperty("status", out _).Should().BeFalse();
    }

    [Fact]
    public void EndToEnd_CustomFieldNames_WithCustomizer()
    {
        var options = new ErrorHandlingOptions();
        options.JsonFieldNames.Code = "errorType";
        options.JsonFieldNames.Message = "errorDetail";

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var errorCodeMapper = new ErrorCodeMapper(optionsWrapper);
        var errorMessageMapper = new ErrorMessageMapper(optionsWrapper);
        var httpStatusMapper = new HttpStatusMapper(optionsWrapper);

        var fallbackHandler = new DefaultFallbackHandler(
            errorCodeMapper, errorMessageMapper, httpStatusMapper, optionsWrapper);

        var customizer = Substitute.For<IApiErrorResponseCustomizer>();
        customizer.When(c => c.Customize(Arg.Any<ApiErrorResponse>()))
            .Do(c => c.Arg<ApiErrorResponse>().AddProperty("traceId", "abc-123"));

        var logger = Substitute.For<ILogger<ErrorHandlingFacade>>();
        var facade = new ErrorHandlingFacade(
            Enumerable.Empty<IApiExceptionHandler>(),
            fallbackHandler,
            new[] { customizer },
            optionsWrapper,
            logger);

        var response = facade.HandleException(new Exception("Test"));
        var jsonOptions = CreateJsonOptions(options.JsonFieldNames);
        var json = JsonSerializer.Serialize(response, jsonOptions);
        var doc = JsonDocument.Parse(json);

        // Custom field names applied
        doc.RootElement.GetProperty("errorType").GetString().Should().NotBeNullOrEmpty();
        doc.RootElement.TryGetProperty("code", out _).Should().BeFalse();

        // Customizer properties preserved
        doc.RootElement.GetProperty("traceId").GetString().Should().Be("abc-123");
    }

    [Fact]
    public void EndToEnd_CustomFieldNames_AllCollectionNames()
    {
        var (facade, options) = CreateFacade(o =>
        {
            o.JsonFieldNames.FieldErrors = "fields";
            o.JsonFieldNames.GlobalErrors = "errors";
            o.JsonFieldNames.ParameterErrors = "params";
        });

        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email is required"));
        response.AddGlobalError(new ApiGlobalError("CROSS_FIELD", "Fields must match"));
        response.AddParameterError(new ApiParameterError("INVALID", "id", "Invalid ID"));

        var jsonOptions = CreateJsonOptions(options.JsonFieldNames);
        var json = JsonSerializer.Serialize(response, jsonOptions);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("fields", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("params", out _).Should().BeTrue();

        // Original names must not exist
        doc.RootElement.TryGetProperty("fieldErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("globalErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("parameterErrors", out _).Should().BeFalse();
    }
}
