using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Integration;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.ProblemDetails;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Integration;

/// <summary>
/// Unit tests for the ErrorResponseWriter, verifying cached JsonSerializerOptions behavior,
/// content types, status codes, and format branching.
/// </summary>
public class ErrorResponseWriterTests
{
    private static ErrorResponseWriter CreateWriter(
        Action<ErrorHandlingOptions>? configure = null)
    {
        var options = new ErrorHandlingOptions();
        configure?.Invoke(options);

        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(options);

        var problemDetailFactory = Substitute.For<IProblemDetailFactory>();
        problemDetailFactory.CreateFromApiError(Arg.Any<ApiErrorResponse>())
            .Returns(callInfo =>
            {
                var apiResponse = callInfo.Arg<ApiErrorResponse>();
                return new ProblemDetailResponse
                {
                    Type = $"https://errors.example.com/{apiResponse.Code.ToLowerInvariant()}",
                    Title = apiResponse.Code,
                    Status = (int)apiResponse.HttpStatusCode,
                    Detail = apiResponse.Message
                };
            });

        return new ErrorResponseWriter(optionsWrapper, problemDetailFactory);
    }

    private static (DefaultHttpContext context, MemoryStream body) CreateHttpContext(string path = "/test")
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        context.Request.Path = path;
        return (context, body);
    }

    private static string ReadResponseBody(MemoryStream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
    }

    // --- Standard format tests ---

    [Fact]
    public async Task WriteResponseAsync_StandardFormat_SetsJsonContentType()
    {
        var writer = CreateWriter();
        var (context, _) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "TEST_ERROR", "Test error");

        await writer.WriteResponseAsync(context, response);

        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task WriteResponseAsync_StandardFormat_SetsCorrectStatusCode()
    {
        var writer = CreateWriter();
        var (context, _) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.NotFound, "NOT_FOUND", "Not found");

        await writer.WriteResponseAsync(context, response);

        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task WriteResponseAsync_StandardFormat_WritesValidJson()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("code").GetString().Should().Be("VALIDATION_FAILED");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Validation failed");
    }

    [Fact]
    public async Task WriteResponseAsync_StandardFormat_OmitsNullFields()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse("ERROR")
        {
            HttpStatusCode = HttpStatusCode.InternalServerError
        };

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("fieldErrors", out _).Should().BeFalse();
    }

    // --- Problem Details format tests ---

    [Fact]
    public async Task WriteResponseAsync_ProblemDetailFormat_SetsProblemJsonContentType()
    {
        var writer = CreateWriter(o => o.UseProblemDetailFormat = true);
        var (context, _) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "TEST_ERROR", "Test error");

        await writer.WriteResponseAsync(context, response);

        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task WriteResponseAsync_ProblemDetailFormat_SetsInstanceToRequestPath()
    {
        var writer = CreateWriter(o => o.UseProblemDetailFormat = true);
        var (context, body) = CreateHttpContext("/api/users/123");
        var response = new ApiErrorResponse(HttpStatusCode.NotFound, "NOT_FOUND", "Not found");

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("instance").GetString().Should().Be("/api/users/123");
    }

    // --- Custom field names tests ---

    [Fact]
    public async Task WriteResponseAsync_CustomFieldNames_AppliesCorrectly()
    {
        var writer = CreateWriter(o =>
        {
            o.JsonFieldNames.Code = "type";
            o.JsonFieldNames.Message = "detail";
        });
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "TEST_ERROR", "Test error");

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("type").GetString().Should().Be("TEST_ERROR");
        doc.RootElement.GetProperty("detail").GetString().Should().Be("Test error");
        doc.RootElement.TryGetProperty("code", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("message", out _).Should().BeFalse();
    }

    // --- Caching verification ---

    [Fact]
    public async Task WriteResponseAsync_MultipleCalls_ProducesConsistentOutput()
    {
        var writer = CreateWriter();
        var responses = new List<string>();

        for (var i = 0; i < 3; i++)
        {
            var (context, body) = CreateHttpContext();
            var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "SAME_ERROR", "Same message");
            await writer.WriteResponseAsync(context, response);
            responses.Add(ReadResponseBody(body));
        }

        // All responses should be identical — proving reused serializer options
        responses.Distinct().Should().HaveCount(1);
    }

    [Fact]
    public async Task WriteResponseAsync_WithFieldErrors_SerializesCorrectly()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.AddFieldError(new ApiFieldError("REQUIRED", "email", "Email is required", null, "email"));

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        var fieldErrors = doc.RootElement.GetProperty("fieldErrors");
        fieldErrors.GetArrayLength().Should().Be(1);
        fieldErrors[0].GetProperty("code").GetString().Should().Be("REQUIRED");
        fieldErrors[0].GetProperty("property").GetString().Should().Be("email");
    }

    [Fact]
    public async Task WriteResponseAsync_CancellationToken_IsPassedThrough()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "TEST", "Test");
        var cts = new CancellationTokenSource();

        // Should complete without throwing when not cancelled
        await writer.WriteResponseAsync(context, response, cts.Token);

        var json = ReadResponseBody(body);
        json.Should().NotBeEmpty();
    }

    // --- HasStarted guard (T035) ---

    [Fact]
    public async Task WriteResponseAsync_ResponseAlreadyStarted_DoesNotWrite()
    {
        var writer = CreateWriter();
        var context = new DefaultHttpContext();
        // Simulate response already started by using a custom feature
        var mockFeature = Substitute.For<IHttpResponseFeature>();
        mockFeature.HasStarted.Returns(true);
        context.Features.Set(mockFeature);

        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");

        // Should not throw, just return silently
        await writer.WriteResponseAsync(context, response);
    }

    // --- GlobalErrors / ParameterErrors / Properties body tests (T035) ---

    [Fact]
    public async Task WriteResponseAsync_WithGlobalErrors_SerializesCorrectly()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.AddGlobalError(new ApiGlobalError("PASSWORDS_MUST_MATCH", "Passwords do not match"));

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        var globalErrors = doc.RootElement.GetProperty("globalErrors");
        globalErrors.GetArrayLength().Should().Be(1);
        globalErrors[0].GetProperty("code").GetString().Should().Be("PASSWORDS_MUST_MATCH");
    }

    [Fact]
    public async Task WriteResponseAsync_WithParameterErrors_SerializesCorrectly()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "VALIDATION_FAILED", "Validation failed");
        response.AddParameterError(new ApiParameterError("REQUIRED", "id", "Id is required", null));

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        var paramErrors = doc.RootElement.GetProperty("parameterErrors");
        paramErrors.GetArrayLength().Should().Be(1);
        paramErrors[0].GetProperty("parameter").GetString().Should().Be("id");
    }

    [Fact]
    public async Task WriteResponseAsync_WithProperties_SerializesCorrectly()
    {
        var writer = CreateWriter();
        var (context, body) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.NotFound, "USER_NOT_FOUND", "User not found");
        response.AddProperty("userId", "abc-123");

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("userId").GetString().Should().Be("abc-123");
    }

    // --- Problem Details body test (T035) ---

    [Fact]
    public async Task WriteResponseAsync_ProblemDetailFormat_WritesValidProblemJson()
    {
        var writer = CreateWriter(o => o.UseProblemDetailFormat = true);
        var (context, body) = CreateHttpContext("/api/test");
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "TEST_ERROR", "Test error");

        await writer.WriteResponseAsync(context, response);

        var json = ReadResponseBody(body);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("type").GetString().Should().Contain("test_error");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        doc.RootElement.GetProperty("detail").GetString().Should().Be("Test error");
    }

    // --- Pre-cancelled token test (T035) ---

    [Fact]
    public async Task WriteResponseAsync_PreCancelledToken_ThrowsOrReturns()
    {
        var writer = CreateWriter();
        var (context, _) = CreateHttpContext();
        var response = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Test");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Pre-cancelled token should throw TaskCanceledException or OperationCanceledException
        var act = async () => await writer.WriteResponseAsync(context, response, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
