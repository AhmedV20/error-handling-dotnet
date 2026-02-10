using System.Text.Json.Serialization;

namespace ErrorLens.ErrorHandling.ProblemDetails;

/// <summary>
/// RFC 9457 Problem Details response model.
/// </summary>
public class ProblemDetailResponse
{
    /// <summary>
    /// A URI reference that identifies the problem type.
    /// When dereferenced, it should provide human-readable documentation.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "about:blank";

    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The HTTP status code for this occurrence of the problem.
    /// </summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem.
    /// </summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// Additional properties that extend the Problem Details response.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Extensions { get; set; } = new();
}
