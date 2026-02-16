using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace ErrorLens.ErrorHandling.OpenApi;

/// <summary>
/// .NET 9+ OpenAPI operation transformer that adds ErrorLens error response schemas
/// to all operations for the configured HTTP status codes.
/// </summary>
public class ErrorResponseOperationTransformer : IOpenApiOperationTransformer
{
    private readonly ErrorResponseSchemaGenerator _schemaGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="ErrorResponseOperationTransformer"/>.
    /// </summary>
    /// <param name="options">The error handling options.</param>
    public ErrorResponseOperationTransformer(ErrorHandlingOptions options)
    {
        _schemaGenerator = new ErrorResponseSchemaGenerator(options);
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Responses ??= new OpenApiResponses();

        var schema = _schemaGenerator.GenerateErrorSchema();
        var contentType = _schemaGenerator.GetContentType();

        foreach (var statusCode in _schemaGenerator.GetDefaultStatusCodes())
        {
            var key = statusCode.ToString();

            // Skip status codes already declared (e.g., via [ProducesResponseType])
            if (operation.Responses.ContainsKey(key))
                continue;

            operation.Responses[key] = new OpenApiResponse
            {
                Description = _schemaGenerator.GetStatusDescription(statusCode),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [contentType] = new OpenApiMediaType { Schema = schema }
                }
            };
        }

        return Task.CompletedTask;
    }
}
