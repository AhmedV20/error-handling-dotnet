using System.Text.Json.Serialization;

namespace ErrorLens.ErrorHandling.Models;

/// <summary>
/// Field-level validation error with rejected value context.
/// </summary>
public class ApiFieldError
{
    /// <summary>
    /// Validation error code (e.g., REQUIRED_NOT_NULL, INVALID_EMAIL).
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Property name that failed validation.
    /// </summary>
    [JsonPropertyName("property")]
    public string Property { get; set; }

    /// <summary>
    /// Validation error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// The value that failed validation.
    /// </summary>
    [JsonPropertyName("rejectedValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? RejectedValue { get; set; }

    /// <summary>
    /// Full property path for nested objects (e.g., user.address.zipCode).
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    public ApiFieldError(string code, string property, string message)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(property);
        Code = code;
        Property = property;
        Message = message;
    }

    public ApiFieldError(string code, string property, string message, object? rejectedValue, string? path = null)
        : this(code, property, message)
    {
        RejectedValue = rejectedValue;
        Path = path;
    }
}
