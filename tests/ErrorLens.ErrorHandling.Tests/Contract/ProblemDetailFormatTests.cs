using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Contract;

/// <summary>
/// Contract tests for RFC 9457 Problem Details format.
/// </summary>
public class ProblemDetailFormatTests
{
    [Fact]
    public void ProblemDetailResponse_ContainsRequiredFields()
    {
        var response = new ProblemDetailResponse
        {
            Type = "https://example.com/errors/validation-error",
            Title = "Bad Request",
            Status = 400,
            Detail = "Validation failed"
        };

        response.Type.Should().NotBeNullOrEmpty();
        response.Title.Should().NotBeNullOrEmpty();
        response.Status.Should().Be(400);
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProblemDetailResponse_SerializesToCorrectJson()
    {
        var response = new ProblemDetailResponse
        {
            Type = "https://example.com/errors/not-found",
            Title = "Not Found",
            Status = 404,
            Detail = "Resource not found",
            Instance = "/api/users/123"
        };

        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<JsonDocument>(json);
        var root = deserialized!.RootElement;

        root.GetProperty("type").GetString().Should().Be("https://example.com/errors/not-found");
        root.GetProperty("title").GetString().Should().Be("Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);
        root.GetProperty("detail").GetString().Should().Be("Resource not found");
        root.GetProperty("instance").GetString().Should().Be("/api/users/123");
    }

    [Fact]
    public void ProblemDetailResponse_SupportsExtensions()
    {
        var response = new ProblemDetailResponse
        {
            Type = "about:blank",
            Title = "Error",
            Status = 500
        };
        response.Extensions["traceId"] = "abc-123";
        response.Extensions["code"] = "INTERNAL_ERROR";

        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<JsonDocument>(json);
        var root = deserialized!.RootElement;

        root.GetProperty("traceId").GetString().Should().Be("abc-123");
        root.GetProperty("code").GetString().Should().Be("INTERNAL_ERROR");
    }

    [Fact]
    public void ProblemDetailFactory_CreatesFromApiError()
    {
        var options = new ErrorHandlingOptions
        {
            ProblemDetailTypePrefix = "https://api.example.com/errors/"
        };
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var factory = new ProblemDetailFactory(optionsWrapper);
        var apiError = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Validation failed");

        var problemDetail = factory.CreateFromApiError(apiError);

        problemDetail.Type.Should().Be("https://api.example.com/errors/validation-error");
        problemDetail.Title.Should().Be("Bad Request");
        problemDetail.Status.Should().Be(400);
        problemDetail.Detail.Should().Be("Validation failed");
        problemDetail.Extensions.Should().ContainKey("code");
        problemDetail.Extensions["code"].Should().Be("VALIDATION_ERROR");
    }
}
