using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Xunit;

namespace ErrorLens.ErrorHandling.OpenApi.Tests;

public class ErrorResponseOperationTransformerTests
{
    private static ErrorResponseOperationTransformer CreateTransformer(ErrorHandlingOptions? options = null)
    {
        var opts = options ?? new ErrorHandlingOptions();
        return new ErrorResponseOperationTransformer(opts);
    }

    // --- Adds error schemas for configured status codes ---

    [Fact]
    public async Task TransformAsync_DefaultStatusCodes_AddsResponsesFor400_404_500()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    [Fact]
    public async Task TransformAsync_CustomStatusCodes_AddsResponsesForConfiguredCodes()
    {
        var options = new ErrorHandlingOptions
        {
            OpenApi = new OpenApiOptions
            {
                DefaultStatusCodes = [400, 401, 422, 500]
            }
        };
        var transformer = CreateTransformer(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("401");
        operation.Responses.Should().ContainKey("422");
        operation.Responses.Should().ContainKey("500");
        operation.Responses.Should().NotContainKey("404");
    }

    [Fact]
    public async Task TransformAsync_DefaultStatusCodes_ResponsesHaveCorrectDescriptions()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses["400"].Description.Should().Be("Bad Request");
        operation.Responses["404"].Description.Should().Be("Not Found");
        operation.Responses["500"].Description.Should().Be("Internal Server Error");
    }

    [Fact]
    public async Task TransformAsync_DefaultStatusCodes_ResponsesHaveSchema()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        foreach (var response in operation.Responses.Values)
        {
            response.Content.Should().NotBeEmpty();
            var mediaType = response.Content.Values.First();
            mediaType.Schema.Should().NotBeNull();
            mediaType.Schema.Type.Should().Be("object");
        }
    }

    // --- Skips already-declared status codes ---

    [Fact]
    public async Task TransformAsync_ExistingResponse_DoesNotOverwrite()
    {
        var transformer = CreateTransformer();
        var existingResponse = new OpenApiResponse { Description = "Custom Bad Request" };
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = existingResponse
            }
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses["400"].Description.Should().Be("Custom Bad Request");
    }

    [Fact]
    public async Task TransformAsync_ExistingResponse_StillAddsOtherCodes()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = new OpenApiResponse { Description = "Custom" }
            }
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    // --- Content type based on UseProblemDetailFormat ---

    [Fact]
    public async Task TransformAsync_StandardFormat_UsesApplicationJson()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        foreach (var response in operation.Responses.Values)
        {
            response.Content.Should().ContainKey("application/json");
        }
    }

    [Fact]
    public async Task TransformAsync_ProblemDetailFormat_UsesApplicationProblemJson()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var transformer = CreateTransformer(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        foreach (var response in operation.Responses.Values)
        {
            response.Content.Should().ContainKey("application/problem+json");
            response.Content.Should().NotContainKey("application/json");
        }
    }

    // --- Schema type based on UseProblemDetailFormat ---

    [Fact]
    public async Task TransformAsync_StandardFormat_SchemaHasStandardProperties()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        var schema = operation.Responses["400"].Content["application/json"].Schema;
        schema.Properties.Should().ContainKey("code");
        schema.Properties.Should().ContainKey("message");
    }

    [Fact]
    public async Task TransformAsync_ProblemDetailFormat_SchemaHasRfc9457Properties()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var transformer = CreateTransformer(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        var schema = operation.Responses["400"].Content["application/problem+json"].Schema;
        schema.Properties.Should().ContainKey("type");
        schema.Properties.Should().ContainKey("title");
        schema.Properties.Should().ContainKey("status");
        schema.Properties.Should().ContainKey("detail");
        schema.Properties.Should().ContainKey("instance");
    }

    // --- Null responses collection ---

    [Fact]
    public async Task TransformAsync_NullResponses_InitializesAndAddsResponses()
    {
        var transformer = CreateTransformer();
        var operation = new OpenApiOperation
        {
            Responses = null
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses.Should().NotBeNull();
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    // --- Custom field names ---

    [Fact]
    public async Task TransformAsync_CustomFieldNames_SchemaReflectsConfiguredNames()
    {
        var options = new ErrorHandlingOptions
        {
            JsonFieldNames = new JsonFieldNamesOptions
            {
                Code = "errorCode",
                Message = "errorMessage"
            }
        };
        var transformer = CreateTransformer(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        var schema = operation.Responses["400"].Content["application/json"].Schema;
        schema.Properties.Should().ContainKey("errorCode");
        schema.Properties.Should().ContainKey("errorMessage");
        schema.Properties.Should().NotContainKey("code");
        schema.Properties.Should().NotContainKey("message");
    }

    // --- Empty status codes ---

    [Fact]
    public async Task TransformAsync_EmptyStatusCodes_AddsNoResponses()
    {
        var options = new ErrorHandlingOptions
        {
            OpenApi = new OpenApiOptions
            {
                DefaultStatusCodes = []
            }
        };
        var transformer = CreateTransformer(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        await transformer.TransformAsync(operation, context: null!, CancellationToken.None);

        operation.Responses.Should().BeEmpty();
    }
}
