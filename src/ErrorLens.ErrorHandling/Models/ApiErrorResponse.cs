using System.Net;
using System.Text.Json.Serialization;

namespace ErrorLens.ErrorHandling.Models;

/// <summary>
/// Primary error response object returned for all exceptions.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Error code in ALL_CAPS format (e.g., USER_NOT_FOUND).
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    /// HTTP status code. Only serialized if HttpStatusInJsonResponse is enabled.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Status { get; set; }

    /// <summary>
    /// Field-level validation errors. Omitted if empty.
    /// </summary>
    [JsonPropertyName("fieldErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiFieldError>? FieldErrors { get; set; }

    /// <summary>
    /// Class-level validation errors. Omitted if empty.
    /// </summary>
    [JsonPropertyName("globalErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiGlobalError>? GlobalErrors { get; set; }

    /// <summary>
    /// Method parameter validation errors. Omitted if empty.
    /// </summary>
    [JsonPropertyName("parameterErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiParameterError>? ParameterErrors { get; set; }

    /// <summary>
    /// Custom properties from [ResponseErrorProperty] attributes.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?>? Properties { get; set; }

    /// <summary>
    /// Internal HTTP status code (not serialized by default).
    /// </summary>
    [JsonIgnore]
    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.InternalServerError;

    public ApiErrorResponse(string code)
    {
        Code = code;
    }

    public ApiErrorResponse(string code, string? message) : this(code)
    {
        Message = message;
    }

    public ApiErrorResponse(HttpStatusCode statusCode, string code, string? message) : this(code, message)
    {
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// Adds a custom property to the error response.
    /// </summary>
    public void AddProperty(string name, object? value)
    {
        Properties ??= new Dictionary<string, object?>();
        Properties[name] = value;
    }

    /// <summary>
    /// Adds a field error to the response.
    /// </summary>
    public void AddFieldError(ApiFieldError fieldError)
    {
        FieldErrors ??= new List<ApiFieldError>();
        FieldErrors.Add(fieldError);
    }

    /// <summary>
    /// Adds a global error to the response.
    /// </summary>
    public void AddGlobalError(ApiGlobalError globalError)
    {
        GlobalErrors ??= new List<ApiGlobalError>();
        GlobalErrors.Add(globalError);
    }

    /// <summary>
    /// Adds a parameter error to the response.
    /// </summary>
    public void AddParameterError(ApiParameterError parameterError)
    {
        ParameterErrors ??= new List<ApiParameterError>();
        ParameterErrors.Add(parameterError);
    }
}
