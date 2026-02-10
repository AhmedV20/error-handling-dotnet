using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Models;

public class ApiErrorResponseTests
{
    [Fact]
    public void Constructor_WithCode_SetsCode()
    {
        var response = new ApiErrorResponse("USER_NOT_FOUND");

        response.Code.Should().Be("USER_NOT_FOUND");
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCodeAndMessage_SetsBoth()
    {
        var response = new ApiErrorResponse("USER_NOT_FOUND", "User was not found");

        response.Code.Should().Be("USER_NOT_FOUND");
        response.Message.Should().Be("User was not found");
    }

    [Fact]
    public void Constructor_WithStatusCodeAndMessage_SetsAll()
    {
        var response = new ApiErrorResponse(HttpStatusCode.NotFound, "USER_NOT_FOUND", "User was not found");

        response.Code.Should().Be("USER_NOT_FOUND");
        response.Message.Should().Be("User was not found");
        response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void AddProperty_AddsToProperties()
    {
        var response = new ApiErrorResponse("ERROR");

        response.AddProperty("userId", "123");
        response.AddProperty("timestamp", 1234567890);

        response.Properties.Should().ContainKey("userId").WhoseValue.Should().Be("123");
        response.Properties.Should().ContainKey("timestamp").WhoseValue.Should().Be(1234567890);
    }

    [Fact]
    public void AddFieldError_AddsToFieldErrors()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        var fieldError = new ApiFieldError("REQUIRED_NOT_NULL", "email", "Email is required");

        response.AddFieldError(fieldError);

        response.FieldErrors.Should().HaveCount(1);
        response.FieldErrors![0].Code.Should().Be("REQUIRED_NOT_NULL");
    }

    [Fact]
    public void AddGlobalError_AddsToGlobalErrors()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        var globalError = new ApiGlobalError("PASSWORDS_MUST_MATCH", "Passwords do not match");

        response.AddGlobalError(globalError);

        response.GlobalErrors.Should().HaveCount(1);
        response.GlobalErrors![0].Code.Should().Be("PASSWORDS_MUST_MATCH");
    }

    [Fact]
    public void AddParameterError_AddsToParameterErrors()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED");
        var paramError = new ApiParameterError("REQUIRED_NOT_NULL", "id", "Id is required");

        response.AddParameterError(paramError);

        response.ParameterErrors.Should().HaveCount(1);
        response.ParameterErrors![0].Parameter.Should().Be("id");
    }

    [Fact]
    public void Serialization_ProducesCorrectJson()
    {
        var response = new ApiErrorResponse("USER_NOT_FOUND", "User was not found");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("USER_NOT_FOUND");
        doc.RootElement.GetProperty("message").GetString().Should().Be("User was not found");
    }

    [Fact]
    public void Serialization_OmitsNullMessage()
    {
        var response = new ApiErrorResponse("ERROR");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
    }

    [Fact]
    public void Serialization_OmitsEmptyArrays()
    {
        var response = new ApiErrorResponse("ERROR", "An error occurred");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("fieldErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("globalErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("parameterErrors", out _).Should().BeFalse();
    }

    [Fact]
    public void Serialization_IncludesCustomProperties()
    {
        var response = new ApiErrorResponse("ERROR", "An error occurred");
        response.AddProperty("userId", "abc123");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("userId").GetString().Should().Be("abc123");
    }

    [Fact]
    public void Serialization_IncludesFieldErrorsWhenPresent()
    {
        var response = new ApiErrorResponse("VALIDATION_FAILED", "Validation failed");
        response.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email is required", null, "email"));

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("fieldErrors", out var fieldErrors).Should().BeTrue();
        fieldErrors.GetArrayLength().Should().Be(1);
        fieldErrors[0].GetProperty("code").GetString().Should().Be("REQUIRED");
    }
}
