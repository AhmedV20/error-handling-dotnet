using System.Text.Json.Serialization;

namespace ErrorLens.ErrorHandling.Models;

/// <summary>
/// Method parameter validation error.
/// </summary>
public class ApiParameterError
{
    /// <summary>
    /// Validation error code.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Parameter name.
    /// </summary>
    [JsonPropertyName("parameter")]
    public string Parameter { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// The value that failed validation.
    /// </summary>
    [JsonPropertyName("rejectedValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? RejectedValue { get; set; }

    public ApiParameterError(string code, string parameter, string message)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(message);
        Code = code;
        Parameter = parameter;
        Message = message;
    }

    public ApiParameterError(string code, string parameter, string message, object? rejectedValue)
        : this(code, parameter, message)
    {
        RejectedValue = rejectedValue;
    }
}
