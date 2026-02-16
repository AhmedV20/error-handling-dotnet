using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.OpenApi;

public class ErrorResponseSchemaGeneratorTests
{
    // --- Standard (ApiErrorResponse) schema tests ---

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ReturnsObjectSchema()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Type.Should().Be("object");
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ContainsCodeProperty()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("code");
        schema.Properties["code"].Type.Should().Be("string");
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ContainsMessageProperty()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("message");
        schema.Properties["message"].Type.Should().Be("string");
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ContainsFieldErrorsArray()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("fieldErrors");
        schema.Properties["fieldErrors"].Type.Should().Be("array");
        schema.Properties["fieldErrors"].Items.Should().NotBeNull();
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ContainsGlobalErrorsArray()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("globalErrors");
        schema.Properties["globalErrors"].Type.Should().Be("array");
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ContainsParameterErrorsArray()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("parameterErrors");
        schema.Properties["parameterErrors"].Type.Should().Be("array");
    }

    [Fact]
    public void GenerateErrorSchema_StandardFormat_CodeIsRequired()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Required.Should().Contain("code");
    }

    [Fact]
    public void GenerateErrorSchema_CustomFieldNames_UsesConfiguredNames()
    {
        var options = new ErrorHandlingOptions
        {
            JsonFieldNames = new JsonFieldNamesOptions
            {
                Code = "errorCode",
                Message = "errorMessage",
                FieldErrors = "fields",
                GlobalErrors = "globals",
                ParameterErrors = "params"
            }
        };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("errorCode");
        schema.Properties.Should().ContainKey("errorMessage");
        schema.Properties.Should().ContainKey("fields");
        schema.Properties.Should().ContainKey("globals");
        schema.Properties.Should().ContainKey("params");
        schema.Properties.Should().NotContainKey("code");
        schema.Properties.Should().NotContainKey("message");
        schema.Required.Should().Contain("errorCode");
    }

    [Fact]
    public void GenerateErrorSchema_HttpStatusInJsonResponseEnabled_ContainsStatusProperty()
    {
        var options = new ErrorHandlingOptions { HttpStatusInJsonResponse = true };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("status");
        schema.Properties["status"].Type.Should().Be("integer");
    }

    [Fact]
    public void GenerateErrorSchema_HttpStatusInJsonResponseDisabled_OmitsStatusProperty()
    {
        var options = new ErrorHandlingOptions { HttpStatusInJsonResponse = false };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().NotContainKey("status");
    }

    // --- ProblemDetails schema tests ---

    [Fact]
    public void GenerateErrorSchema_ProblemDetailFormat_ReturnsObjectSchema()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Type.Should().Be("object");
    }

    [Fact]
    public void GenerateErrorSchema_ProblemDetailFormat_ContainsRfc9457Properties()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        schema.Properties.Should().ContainKey("type");
        schema.Properties["type"].Type.Should().Be("string");
        schema.Properties.Should().ContainKey("title");
        schema.Properties["title"].Type.Should().Be("string");
        schema.Properties.Should().ContainKey("status");
        schema.Properties["status"].Type.Should().Be("integer");
        schema.Properties.Should().ContainKey("detail");
        schema.Properties["detail"].Type.Should().Be("string");
        schema.Properties.Should().ContainKey("instance");
        schema.Properties["instance"].Type.Should().Be("string");
    }

    [Fact]
    public void GenerateErrorSchema_ProblemDetailFormat_DoesNotContainStandardFieldNames()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        // ProblemDetails uses RFC 9457 names, not custom JsonFieldNames
        schema.Properties.Should().NotContainKey("code");
        schema.Properties.Should().NotContainKey("message");
        schema.Properties.Should().NotContainKey("fieldErrors");
    }

    // --- DefaultStatusCodes tests ---

    [Fact]
    public void GetDefaultStatusCodes_ReturnsConfiguredCodes()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var codes = generator.GetDefaultStatusCodes();

        codes.Should().BeEquivalentTo(new[] { 400, 404, 500 });
    }

    [Fact]
    public void GetDefaultStatusCodes_CustomCodes_ReturnsConfiguredCodes()
    {
        var options = new ErrorHandlingOptions
        {
            OpenApi = new OpenApiOptions
            {
                DefaultStatusCodes = [400, 401, 403, 404, 422, 500]
            }
        };
        var generator = new ErrorResponseSchemaGenerator(options);

        var codes = generator.GetDefaultStatusCodes();

        codes.Should().BeEquivalentTo(new[] { 400, 401, 403, 404, 422, 500 });
    }

    // --- Content type tests ---

    [Fact]
    public void GetContentType_StandardFormat_ReturnsApplicationJson()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var contentType = generator.GetContentType();

        contentType.Should().Be("application/json");
    }

    [Fact]
    public void GetContentType_ProblemDetailFormat_ReturnsApplicationProblemJson()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var generator = new ErrorResponseSchemaGenerator(options);

        var contentType = generator.GetContentType();

        contentType.Should().Be("application/problem+json");
    }

    // --- FieldError sub-schema tests ---

    [Fact]
    public void GenerateErrorSchema_StandardFormat_FieldErrorItemsHaveCorrectProperties()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        var fieldErrorSchema = schema.Properties["fieldErrors"].Items;
        fieldErrorSchema.Properties.Should().ContainKey("code");
        fieldErrorSchema.Properties.Should().ContainKey("property");
        fieldErrorSchema.Properties.Should().ContainKey("message");
        fieldErrorSchema.Properties.Should().ContainKey("rejectedValue");
        fieldErrorSchema.Properties.Should().ContainKey("path");
    }

    [Fact]
    public void GenerateErrorSchema_CustomFieldNames_FieldErrorUsesConfiguredNames()
    {
        var options = new ErrorHandlingOptions
        {
            JsonFieldNames = new JsonFieldNamesOptions
            {
                Property = "fieldName",
                RejectedValue = "invalidValue",
                Path = "jsonPath"
            }
        };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        var fieldErrorSchema = schema.Properties["fieldErrors"].Items;
        fieldErrorSchema.Properties.Should().ContainKey("fieldName");
        fieldErrorSchema.Properties.Should().ContainKey("invalidValue");
        fieldErrorSchema.Properties.Should().ContainKey("jsonPath");
        fieldErrorSchema.Properties.Should().NotContainKey("property");
        fieldErrorSchema.Properties.Should().NotContainKey("rejectedValue");
        fieldErrorSchema.Properties.Should().NotContainKey("path");
    }

    // --- GlobalError sub-schema tests ---

    [Fact]
    public void GenerateErrorSchema_StandardFormat_GlobalErrorItemsHaveCorrectProperties()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        var globalErrorSchema = schema.Properties["globalErrors"].Items;
        globalErrorSchema.Properties.Should().ContainKey("code");
        globalErrorSchema.Properties.Should().ContainKey("message");
    }

    // --- ParameterError sub-schema tests ---

    [Fact]
    public void GenerateErrorSchema_StandardFormat_ParameterErrorItemsHaveCorrectProperties()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        var paramErrorSchema = schema.Properties["parameterErrors"].Items;
        paramErrorSchema.Properties.Should().ContainKey("code");
        paramErrorSchema.Properties.Should().ContainKey("parameter");
        paramErrorSchema.Properties.Should().ContainKey("message");
        paramErrorSchema.Properties.Should().ContainKey("rejectedValue");
    }

    [Fact]
    public void GenerateErrorSchema_CustomFieldNames_ParameterErrorUsesConfiguredNames()
    {
        var options = new ErrorHandlingOptions
        {
            JsonFieldNames = new JsonFieldNamesOptions
            {
                Parameter = "paramName",
                RejectedValue = "invalidValue"
            }
        };
        var generator = new ErrorResponseSchemaGenerator(options);

        var schema = generator.GenerateErrorSchema();

        var paramErrorSchema = schema.Properties["parameterErrors"].Items;
        paramErrorSchema.Properties.Should().ContainKey("paramName");
        paramErrorSchema.Properties.Should().ContainKey("invalidValue");
        paramErrorSchema.Properties.Should().NotContainKey("parameter");
        paramErrorSchema.Properties.Should().NotContainKey("rejectedValue");
    }

    // --- GetStatusDescription tests ---

    [Fact]
    public void GetStatusDescription_400_ReturnsBadRequest()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        generator.GetStatusDescription(400).Should().Be("Bad Request");
    }

    [Fact]
    public void GetStatusDescription_404_ReturnsNotFound()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        generator.GetStatusDescription(404).Should().Be("Not Found");
    }

    [Fact]
    public void GetStatusDescription_500_ReturnsInternalServerError()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        generator.GetStatusDescription(500).Should().Be("Internal Server Error");
    }

    [Fact]
    public void GetStatusDescription_UnknownCode_ReturnsErrorFallback()
    {
        var options = new ErrorHandlingOptions();
        var generator = new ErrorResponseSchemaGenerator(options);

        generator.GetStatusDescription(418).Should().NotBeNullOrEmpty();
    }
}
