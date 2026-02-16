using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using Microsoft.OpenApi.Models;

namespace ErrorLens.ErrorHandling.OpenApi;

/// <summary>
/// Generates OpenAPI schema objects for ErrorLens error responses.
/// Supports both standard <c>ApiErrorResponse</c> and RFC 9457 Problem Details formats.
/// </summary>
internal class ErrorResponseSchemaGenerator
{
    private readonly ErrorHandlingOptions _options;

    public ErrorResponseSchemaGenerator(ErrorHandlingOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Generates the OpenAPI schema for the error response based on current configuration.
    /// </summary>
    public OpenApiSchema GenerateErrorSchema()
    {
        return _options.UseProblemDetailFormat
            ? GenerateProblemDetailSchema()
            : GenerateStandardSchema();
    }

    /// <summary>
    /// Returns the configured default status codes for error responses.
    /// </summary>
    public HashSet<int> GetDefaultStatusCodes() => _options.OpenApi.DefaultStatusCodes;

    /// <summary>
    /// Returns the appropriate content type based on the configured format.
    /// </summary>
    public string GetContentType()
    {
        return _options.UseProblemDetailFormat
            ? "application/problem+json"
            : "application/json";
    }

    /// <summary>
    /// Returns a human-readable description for the given HTTP status code.
    /// </summary>
    public string GetStatusDescription(int statusCode)
    {
        return ((HttpStatusCode)statusCode) switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
            HttpStatusCode.TooManyRequests => "Too Many Requests",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout",
            _ => "Error"
        };
    }

    private OpenApiSchema GenerateStandardSchema()
    {
        var fieldNames = _options.JsonFieldNames;

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [fieldNames.Code] = new OpenApiSchema { Type = "string", Description = "Error code" },
                [fieldNames.Message] = new OpenApiSchema { Type = "string", Description = "Human-readable error message" },
                [fieldNames.FieldErrors] = new OpenApiSchema
                {
                    Type = "array",
                    Description = "Field-level validation errors",
                    Items = GenerateFieldErrorSchema()
                },
                [fieldNames.GlobalErrors] = new OpenApiSchema
                {
                    Type = "array",
                    Description = "Class-level validation errors",
                    Items = GenerateGlobalErrorSchema()
                },
                [fieldNames.ParameterErrors] = new OpenApiSchema
                {
                    Type = "array",
                    Description = "Parameter validation errors",
                    Items = GenerateParameterErrorSchema()
                }
            },
            Required = new HashSet<string> { fieldNames.Code }
        };

        if (_options.HttpStatusInJsonResponse)
        {
            schema.Properties[fieldNames.Status] = new OpenApiSchema
            {
                Type = "integer",
                Description = "HTTP status code"
            };
        }

        return schema;
    }

    private OpenApiSchema GenerateProblemDetailSchema()
    {
        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = "string", Description = "URI reference identifying the problem type" },
                ["title"] = new OpenApiSchema { Type = "string", Description = "Short human-readable summary" },
                ["status"] = new OpenApiSchema { Type = "integer", Description = "HTTP status code" },
                ["detail"] = new OpenApiSchema { Type = "string", Description = "Human-readable explanation" },
                ["instance"] = new OpenApiSchema { Type = "string", Description = "URI reference identifying the specific occurrence" }
            },
            Required = new HashSet<string> { "type", "status" }
        };
    }

    private OpenApiSchema GenerateFieldErrorSchema()
    {
        var fieldNames = _options.JsonFieldNames;

        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [fieldNames.Code] = new OpenApiSchema { Type = "string" },
                [fieldNames.Property] = new OpenApiSchema { Type = "string" },
                [fieldNames.Message] = new OpenApiSchema { Type = "string" },
                [fieldNames.RejectedValue] = new OpenApiSchema { Description = "The value that failed validation" },
                [fieldNames.Path] = new OpenApiSchema { Type = "string" }
            },
            Required = new HashSet<string> { fieldNames.Code, fieldNames.Property, fieldNames.Message }
        };
    }

    private OpenApiSchema GenerateGlobalErrorSchema()
    {
        var fieldNames = _options.JsonFieldNames;

        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [fieldNames.Code] = new OpenApiSchema { Type = "string" },
                [fieldNames.Message] = new OpenApiSchema { Type = "string" }
            },
            Required = new HashSet<string> { fieldNames.Code, fieldNames.Message }
        };
    }

    private OpenApiSchema GenerateParameterErrorSchema()
    {
        var fieldNames = _options.JsonFieldNames;

        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [fieldNames.Code] = new OpenApiSchema { Type = "string" },
                [fieldNames.Parameter] = new OpenApiSchema { Type = "string" },
                [fieldNames.Message] = new OpenApiSchema { Type = "string" },
                [fieldNames.RejectedValue] = new OpenApiSchema { Description = "The value that failed validation" }
            },
            Required = new HashSet<string> { fieldNames.Code, fieldNames.Parameter, fieldNames.Message }
        };
    }
}
