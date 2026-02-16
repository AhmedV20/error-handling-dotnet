using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ErrorLens.ErrorHandling.Swashbuckle;

/// <summary>
/// Swashbuckle operation filter that adds ErrorLens error response schemas
/// to all operations for the configured HTTP status codes.
/// </summary>
public class ErrorResponseOperationFilter : IOperationFilter
{
    private readonly ErrorResponseSchemaGenerator _schemaGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="ErrorResponseOperationFilter"/>.
    /// </summary>
    /// <param name="options">The error handling options.</param>
    public ErrorResponseOperationFilter(ErrorHandlingOptions options)
    {
        _schemaGenerator = new ErrorResponseSchemaGenerator(options);
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
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
    }
}
