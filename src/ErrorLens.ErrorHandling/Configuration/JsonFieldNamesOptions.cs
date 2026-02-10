namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Options for customizing JSON property names in error responses.
/// </summary>
public class JsonFieldNamesOptions
{
    // --- ApiErrorResponse fields ---

    /// <summary>
    /// Property name for error code. Default: "code"
    /// </summary>
    public string Code { get; set; } = "code";

    /// <summary>
    /// Property name for error message. Default: "message"
    /// </summary>
    public string Message { get; set; } = "message";

    /// <summary>
    /// Property name for HTTP status code. Default: "status"
    /// </summary>
    public string Status { get; set; } = "status";

    /// <summary>
    /// Property name for field errors array. Default: "fieldErrors"
    /// </summary>
    public string FieldErrors { get; set; } = "fieldErrors";

    /// <summary>
    /// Property name for global errors array. Default: "globalErrors"
    /// </summary>
    public string GlobalErrors { get; set; } = "globalErrors";

    /// <summary>
    /// Property name for parameter errors array. Default: "parameterErrors"
    /// </summary>
    public string ParameterErrors { get; set; } = "parameterErrors";

    // --- Nested error model fields ---

    /// <summary>
    /// Property name for the field/property name in field errors. Default: "property"
    /// </summary>
    public string Property { get; set; } = "property";

    /// <summary>
    /// Property name for rejected value in field/parameter errors. Default: "rejectedValue"
    /// </summary>
    public string RejectedValue { get; set; } = "rejectedValue";

    /// <summary>
    /// Property name for property path in field errors. Default: "path"
    /// </summary>
    public string Path { get; set; } = "path";

    /// <summary>
    /// Property name for parameter name in parameter errors. Default: "parameter"
    /// </summary>
    public string Parameter { get; set; } = "parameter";
}
