namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Strategy for generating error codes from exception types.
/// </summary>
public enum ErrorCodeStrategy
{
    /// <summary>
    /// Convert exception class name to ALL_CAPS format.
    /// Example: UserNotFoundException → USER_NOT_FOUND
    /// </summary>
    AllCaps,

    /// <summary>
    /// Use full qualified class name.
    /// Example: MyApp.Exceptions.UserNotFoundException
    /// </summary>
    FullQualifiedName
}
