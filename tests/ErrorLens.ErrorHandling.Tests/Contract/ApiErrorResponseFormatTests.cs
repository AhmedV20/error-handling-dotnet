using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Contract;

/// <summary>
/// Contract tests to verify JSON response format matches specification.
/// </summary>
public class ApiErrorResponseFormatTests
{
    [Fact]
    public void ErrorResponse_MatchesContractSchema_MinimalResponse()
    {
        var response = new ApiErrorResponse("USER_NOT_FOUND");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        // Required field
        doc.RootElement.GetProperty("code").GetString().Should().Be("USER_NOT_FOUND");

        // Optional fields should be omitted when null/empty
        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("fieldErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("globalErrors", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("parameterErrors", out _).Should().BeFalse();
    }

    [Fact]
    public void ErrorResponse_MatchesContractSchema_FullResponse()
    {
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.Status = 400;
        response.AddFieldError(new ApiFieldError("REQUIRED_NOT_NULL", "email", "Email is required", null, "email"));
        response.AddGlobalError(new ApiGlobalError("CROSS_FIELD", "Fields must match"));
        response.AddParameterError(new ApiParameterError("INVALID", "id", "Invalid ID", "abc"));
        response.AddProperty("timestamp", "2026-02-09T12:00:00Z");

        var json = JsonSerializer.Serialize(response);
        var doc = JsonDocument.Parse(json);

        // Verify structure
        doc.RootElement.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Validation failed");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(400);

        // Verify fieldErrors array
        var fieldErrors = doc.RootElement.GetProperty("fieldErrors");
        fieldErrors.GetArrayLength().Should().Be(1);
        fieldErrors[0].GetProperty("code").GetString().Should().Be("REQUIRED_NOT_NULL");
        fieldErrors[0].GetProperty("property").GetString().Should().Be("email");
        fieldErrors[0].GetProperty("message").GetString().Should().Be("Email is required");
        fieldErrors[0].GetProperty("path").GetString().Should().Be("email");

        // Verify globalErrors array
        var globalErrors = doc.RootElement.GetProperty("globalErrors");
        globalErrors.GetArrayLength().Should().Be(1);
        globalErrors[0].GetProperty("code").GetString().Should().Be("CROSS_FIELD");

        // Verify parameterErrors array
        var parameterErrors = doc.RootElement.GetProperty("parameterErrors");
        parameterErrors.GetArrayLength().Should().Be(1);
        parameterErrors[0].GetProperty("parameter").GetString().Should().Be("id");

        // Verify custom property
        doc.RootElement.GetProperty("timestamp").GetString().Should().Be("2026-02-09T12:00:00Z");
    }

    [Fact]
    public void ErrorCode_FollowsAllCapsPattern()
    {
        var validCodes = new[] { "USER_NOT_FOUND", "VALIDATION_FAILED", "ACCESS_DENIED", "INTERNAL_SERVER_ERROR" };

        foreach (var code in validCodes)
        {
            code.Should().MatchRegex("^[A-Z][A-Z0-9_]*$", "Error codes must be ALL_CAPS format");
        }
    }

    [Fact]
    public void FieldError_ContainsRequiredFields()
    {
        var fieldError = new ApiFieldError("REQUIRED", "email", "Email is required");

        var json = JsonSerializer.Serialize(fieldError);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("property").GetString().Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
    }
}
