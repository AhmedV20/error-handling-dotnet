using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Serialization;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Serialization;

public class ApiErrorResponseConverterTests
{
    private static JsonSerializerOptions CreateOptions(JsonFieldNamesOptions? fieldNames = null)
    {
        fieldNames ??= new JsonFieldNamesOptions();
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ApiErrorResponseConverter(fieldNames) }
        };
    }

    // --- Default field names (backward compatibility) ---

    [Fact]
    public void Write_DefaultNames_ProducesExpectedJson()
    {
        var response = new ApiErrorResponse("USER_NOT_FOUND", "User was not found");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("USER_NOT_FOUND");
        doc.RootElement.GetProperty("message").GetString().Should().Be("User was not found");
    }

    [Fact]
    public void Write_DefaultNames_OmitsNullMessage()
    {
        var response = new ApiErrorResponse("ERROR");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_DefaultNames_OmitsZeroStatus()
    {
        var response = new ApiErrorResponse("ERROR");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("status", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_DefaultNames_IncludesNonZeroStatus()
    {
        var response = new ApiErrorResponse("ERROR") { Status = 500 };
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("status").GetInt32().Should().Be(500);
    }

    [Fact]
    public void Write_DefaultNames_OmitsNullCollections()
    {
        var response = new ApiErrorResponse("ERROR", "An error");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("fieldErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("globalErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("parameterErrors", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_DefaultNames_FullResponse()
    {
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.Status = 400;
        response.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email is required", null, "email"));
        response.AddGlobalError(new ApiGlobalError("CROSS_FIELD", "Fields must match"));
        response.AddParameterError(new ApiParameterError("INVALID", "id", "Invalid ID", "abc"));
        response.AddProperty("traceId", "abc-123");

        var options = CreateOptions();
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Validation failed");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(400);

        var fieldErrors = doc.RootElement.GetProperty("fieldErrors");
        fieldErrors.GetArrayLength().Should().Be(1);
        fieldErrors[0].GetProperty("code").GetString().Should().Be("REQUIRED");
        fieldErrors[0].GetProperty("property").GetString().Should().Be("email");
        fieldErrors[0].GetProperty("message").GetString().Should().Be("Email is required");
        fieldErrors[0].GetProperty("path").GetString().Should().Be("email");

        var globalErrors = doc.RootElement.GetProperty("globalErrors");
        globalErrors.GetArrayLength().Should().Be(1);
        globalErrors[0].GetProperty("code").GetString().Should().Be("CROSS_FIELD");
        globalErrors[0].GetProperty("message").GetString().Should().Be("Fields must match");

        var parameterErrors = doc.RootElement.GetProperty("parameterErrors");
        parameterErrors.GetArrayLength().Should().Be(1);
        parameterErrors[0].GetProperty("code").GetString().Should().Be("INVALID");
        parameterErrors[0].GetProperty("parameter").GetString().Should().Be("id");
        parameterErrors[0].GetProperty("message").GetString().Should().Be("Invalid ID");
        parameterErrors[0].GetProperty("rejectedValue").GetString().Should().Be("abc");

        doc.RootElement.GetProperty("traceId").GetString().Should().Be("abc-123");
    }

    // --- Custom field names ---

    [Fact]
    public void Write_CustomNames_UsesConfiguredNames()
    {
        var fieldNames = new JsonFieldNamesOptions
        {
            Code = "type",
            Message = "detail",
            Status = "statusCode"
        };
        var response = new ApiErrorResponse("USER_NOT_FOUND", "User was not found") { Status = 404 };
        var options = CreateOptions(fieldNames);

        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("type").GetString().Should().Be("USER_NOT_FOUND");
        doc.RootElement.GetProperty("detail").GetString().Should().Be("User was not found");
        doc.RootElement.GetProperty("statusCode").GetInt32().Should().Be(404);

        // Original names should NOT exist
        doc.RootElement.TryGetProperty("code", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("status", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_CustomNames_FieldErrors_UsesConfiguredNames()
    {
        var fieldNames = new JsonFieldNamesOptions
        {
            Code = "type",
            Message = "detail",
            FieldErrors = "fields",
            Property = "field",
            RejectedValue = "value",
            Path = "jsonPath"
        };
        var response = new ApiErrorResponse("VALIDATION_FAILED", "Validation failed");
        response.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email is required", "bad@", "user.email"));

        var options = CreateOptions(fieldNames);
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        var fields = doc.RootElement.GetProperty("fields");
        fields.GetArrayLength().Should().Be(1);
        fields[0].GetProperty("type").GetString().Should().Be("REQUIRED");
        fields[0].GetProperty("field").GetString().Should().Be("email");
        fields[0].GetProperty("detail").GetString().Should().Be("Email is required");
        fields[0].GetProperty("value").GetString().Should().Be("bad@");
        fields[0].GetProperty("jsonPath").GetString().Should().Be("user.email");

        // Original names should NOT exist in nested objects
        fields[0].TryGetProperty("code", out _).Should().BeFalse();
        fields[0].TryGetProperty("property", out _).Should().BeFalse();
        fields[0].TryGetProperty("message", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_CustomNames_GlobalErrors_UsesConfiguredNames()
    {
        var fieldNames = new JsonFieldNamesOptions
        {
            Code = "errorCode",
            Message = "errorMessage",
            GlobalErrors = "errors"
        };
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        response.AddGlobalError(new ApiGlobalError("PASSWORDS_MUST_MATCH", "Passwords do not match"));

        var options = CreateOptions(fieldNames);
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        var errors = doc.RootElement.GetProperty("errors");
        errors.GetArrayLength().Should().Be(1);
        errors[0].GetProperty("errorCode").GetString().Should().Be("PASSWORDS_MUST_MATCH");
        errors[0].GetProperty("errorMessage").GetString().Should().Be("Passwords do not match");
    }

    [Fact]
    public void Write_CustomNames_ParameterErrors_UsesConfiguredNames()
    {
        var fieldNames = new JsonFieldNamesOptions
        {
            Code = "errorType",
            Message = "description",
            ParameterErrors = "params",
            Parameter = "paramName",
            RejectedValue = "badValue"
        };
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        response.AddParameterError(new ApiParameterError("INVALID", "id", "Invalid ID", "abc"));

        var options = CreateOptions(fieldNames);
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        var paramErrors = doc.RootElement.GetProperty("params");
        paramErrors.GetArrayLength().Should().Be(1);
        paramErrors[0].GetProperty("errorType").GetString().Should().Be("INVALID");
        paramErrors[0].GetProperty("paramName").GetString().Should().Be("id");
        paramErrors[0].GetProperty("description").GetString().Should().Be("Invalid ID");
        paramErrors[0].GetProperty("badValue").GetString().Should().Be("abc");
    }

    // --- Extension data / properties ---

    [Fact]
    public void Write_ExtensionData_PreservedWithCustomNames()
    {
        var fieldNames = new JsonFieldNamesOptions { Code = "type", Message = "detail" };
        var response = new ApiErrorResponse("ERROR", "An error");
        response.AddProperty("traceId", "trace-123");
        response.AddProperty("timestamp", "2026-02-10T12:00:00Z");

        var options = CreateOptions(fieldNames);
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("traceId").GetString().Should().Be("trace-123");
        doc.RootElement.GetProperty("timestamp").GetString().Should().Be("2026-02-10T12:00:00Z");
    }

    [Fact]
    public void Write_EmptyProperties_NotSerialized()
    {
        var response = new ApiErrorResponse("ERROR");
        response.Properties = new Dictionary<string, object?>();

        var options = CreateOptions();
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        // Only "code" should be present
        doc.RootElement.EnumerateObject().Should().HaveCount(1);
        doc.RootElement.GetProperty("code").GetString().Should().Be("ERROR");
    }

    // --- Field error edge cases ---

    [Fact]
    public void Write_FieldError_OmitsNullRejectedValue()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        response.AddFieldError(new ApiFieldError("REQUIRED", "name", "Name is required"));

        var options = CreateOptions();
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        var fieldErrors = doc.RootElement.GetProperty("fieldErrors");
        fieldErrors[0].TryGetProperty("rejectedValue", out _).Should().BeFalse();
        fieldErrors[0].TryGetProperty("path", out _).Should().BeFalse();
    }

    [Fact]
    public void Write_ParameterError_OmitsNullRejectedValue()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        response.AddParameterError(new ApiParameterError("REQUIRED", "id", "Id is required"));

        var options = CreateOptions();
        var json = JsonSerializer.Serialize(response, options);
        var doc = JsonDocument.Parse(json);

        var paramErrors = doc.RootElement.GetProperty("parameterErrors");
        paramErrors[0].TryGetProperty("rejectedValue", out _).Should().BeFalse();
    }

    // --- Read (deserialization) ---

    [Fact]
    public void Read_DefaultNames_DeserializesCorrectly()
    {
        var json = """{"code":"USER_NOT_FOUND","message":"User was not found"}""";
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<ApiErrorResponse>(json, options);

        result.Should().NotBeNull();
        result!.Code.Should().Be("USER_NOT_FOUND");
        result.Message.Should().Be("User was not found");
    }

    [Fact]
    public void Read_CustomNames_DeserializesCorrectly()
    {
        var fieldNames = new JsonFieldNamesOptions { Code = "type", Message = "detail" };
        var json = """{"type":"USER_NOT_FOUND","detail":"User was not found"}""";
        var options = CreateOptions(fieldNames);

        var result = JsonSerializer.Deserialize<ApiErrorResponse>(json, options);

        result.Should().NotBeNull();
        result!.Code.Should().Be("USER_NOT_FOUND");
        result.Message.Should().Be("User was not found");
    }

    [Fact]
    public void Read_MissingFields_ReturnsDefaults()
    {
        var json = """{}""";
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<ApiErrorResponse>(json, options);

        result.Should().NotBeNull();
        result!.Code.Should().Be("");
        result.Message.Should().BeNull();
    }

    // --- Round-trip ---

    [Fact]
    public void RoundTrip_DefaultNames_PreservesData()
    {
        var original = new ApiErrorResponse("TEST_ERROR", "Test message");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<ApiErrorResponse>(json, options);

        deserialized.Should().NotBeNull();
        deserialized!.Code.Should().Be(original.Code);
        deserialized.Message.Should().Be(original.Message);
    }
}
