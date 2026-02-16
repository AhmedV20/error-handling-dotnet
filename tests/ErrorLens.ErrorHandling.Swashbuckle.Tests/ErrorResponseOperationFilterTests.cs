using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using ErrorLens.ErrorHandling.Swashbuckle;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Xunit;

namespace ErrorLens.ErrorHandling.Swashbuckle.Tests;

public class ErrorResponseOperationFilterTests
{
    private static ErrorResponseOperationFilter CreateFilter(ErrorHandlingOptions? options = null)
    {
        var opts = options ?? new ErrorHandlingOptions();
        return new ErrorResponseOperationFilter(opts);
    }

    // --- Adds error schemas for configured status codes ---

    [Fact]
    public void Apply_DefaultStatusCodes_AddsResponsesFor400_404_500()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    [Fact]
    public void Apply_CustomStatusCodes_AddsResponsesForConfiguredCodes()
    {
        var options = new ErrorHandlingOptions
        {
            OpenApi = new OpenApiOptions
            {
                DefaultStatusCodes = [400, 401, 422, 500]
            }
        };
        var filter = CreateFilter(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("401");
        operation.Responses.Should().ContainKey("422");
        operation.Responses.Should().ContainKey("500");
        operation.Responses.Should().NotContainKey("404");
    }

    [Fact]
    public void Apply_DefaultStatusCodes_ResponsesHaveCorrectDescriptions()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        operation.Responses["400"].Description.Should().Be("Bad Request");
        operation.Responses["404"].Description.Should().Be("Not Found");
        operation.Responses["500"].Description.Should().Be("Internal Server Error");
    }

    [Fact]
    public void Apply_DefaultStatusCodes_ResponsesHaveSchema()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

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
    public void Apply_ExistingResponse_DoesNotOverwrite()
    {
        var filter = CreateFilter();
        var existingResponse = new OpenApiResponse { Description = "Custom Bad Request" };
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = existingResponse
            }
        };

        filter.Apply(operation, context: null!);

        operation.Responses["400"].Description.Should().Be("Custom Bad Request");
    }

    [Fact]
    public void Apply_ExistingResponse_StillAddsOtherCodes()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = new OpenApiResponse { Description = "Custom" }
            }
        };

        filter.Apply(operation, context: null!);

        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    // --- Content type based on UseProblemDetailFormat ---

    [Fact]
    public void Apply_StandardFormat_UsesApplicationJson()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        foreach (var response in operation.Responses.Values)
        {
            response.Content.Should().ContainKey("application/json");
        }
    }

    [Fact]
    public void Apply_ProblemDetailFormat_UsesApplicationProblemJson()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var filter = CreateFilter(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        foreach (var response in operation.Responses.Values)
        {
            response.Content.Should().ContainKey("application/problem+json");
            response.Content.Should().NotContainKey("application/json");
        }
    }

    // --- Schema type based on UseProblemDetailFormat ---

    [Fact]
    public void Apply_StandardFormat_SchemaHasStandardProperties()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        var schema = operation.Responses["400"].Content["application/json"].Schema;
        schema.Properties.Should().ContainKey("code");
        schema.Properties.Should().ContainKey("message");
    }

    [Fact]
    public void Apply_ProblemDetailFormat_SchemaHasRfc9457Properties()
    {
        var options = new ErrorHandlingOptions { UseProblemDetailFormat = true };
        var filter = CreateFilter(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        var schema = operation.Responses["400"].Content["application/problem+json"].Schema;
        schema.Properties.Should().ContainKey("type");
        schema.Properties.Should().ContainKey("title");
        schema.Properties.Should().ContainKey("status");
        schema.Properties.Should().ContainKey("detail");
        schema.Properties.Should().ContainKey("instance");
    }

    // --- Null responses collection ---

    [Fact]
    public void Apply_NullResponses_InitializesAndAddsResponses()
    {
        var filter = CreateFilter();
        var operation = new OpenApiOperation
        {
            Responses = null
        };

        filter.Apply(operation, context: null!);

        operation.Responses.Should().NotBeNull();
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    // --- Custom field names ---

    [Fact]
    public void Apply_CustomFieldNames_SchemaReflectsConfiguredNames()
    {
        var options = new ErrorHandlingOptions
        {
            JsonFieldNames = new JsonFieldNamesOptions
            {
                Code = "errorCode",
                Message = "errorMessage"
            }
        };
        var filter = CreateFilter(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        var schema = operation.Responses["400"].Content["application/json"].Schema;
        schema.Properties.Should().ContainKey("errorCode");
        schema.Properties.Should().ContainKey("errorMessage");
        schema.Properties.Should().NotContainKey("code");
        schema.Properties.Should().NotContainKey("message");
    }

    // --- Empty status codes ---

    [Fact]
    public void Apply_EmptyStatusCodes_AddsNoResponses()
    {
        var options = new ErrorHandlingOptions
        {
            OpenApi = new OpenApiOptions
            {
                DefaultStatusCodes = []
            }
        };
        var filter = CreateFilter(options);
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };

        filter.Apply(operation, context: null!);

        operation.Responses.Should().BeEmpty();
    }
}
