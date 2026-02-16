namespace ErrorLens.ErrorHandling.OpenApi;

/// <summary>
/// Configuration options for OpenAPI/Swagger error response schema generation.
/// </summary>
public class OpenApiOptions
{
    /// <summary>
    /// HTTP status codes to add error response schemas for.
    /// Default: {400, 404, 500}
    /// </summary>
    public HashSet<int> DefaultStatusCodes { get; set; } = [400, 404, 500];
}
