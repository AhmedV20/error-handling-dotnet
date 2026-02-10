namespace ErrorLens.ErrorHandling.Attributes;

/// <summary>
/// Marks a property or method to be included in the error response.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ResponseErrorPropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance with default settings.
    /// </summary>
    public ResponseErrorPropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom property name.
    /// </summary>
    /// <param name="name">The name to use in the error response.</param>
    public ResponseErrorPropertyAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Custom name for the property in the response. If null, uses the member name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether to include the property even if its value is null. Default: false.
    /// </summary>
    public bool IncludeIfNull { get; set; } = false;
}
