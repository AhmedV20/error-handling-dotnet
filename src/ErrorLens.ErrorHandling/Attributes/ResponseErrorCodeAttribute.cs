namespace ErrorLens.ErrorHandling.Attributes;

/// <summary>
/// Specifies a custom error code for an exception class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ResponseErrorCodeAttribute : Attribute
{
    /// <summary>
    /// The error code to use for this exception.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Creates a new ResponseErrorCodeAttribute with the specified code.
    /// </summary>
    /// <param name="code">The error code (e.g., "USER_NOT_FOUND").</param>
    public ResponseErrorCodeAttribute(string code)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}
