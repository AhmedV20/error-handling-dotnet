using System.Text.Json.Serialization;

namespace ErrorLens.ErrorHandling.Models;

/// <summary>
/// Class-level validation error (no specific field).
/// </summary>
public class ApiGlobalError
{
    /// <summary>
    /// Error code.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ApiGlobalError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
