using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Configuration;

/// <summary>
/// Validates <see cref="ErrorHandlingOptions"/> at startup, ensuring all
/// <see cref="JsonFieldNamesOptions"/> properties are non-null, non-empty, and unique.
/// </summary>
internal class ErrorHandlingOptionsValidator : IValidateOptions<ErrorHandlingOptions>
{
    public ValidateOptionsResult Validate(string? name, ErrorHandlingOptions options)
    {
        var fieldNames = options.JsonFieldNames;
        var failures = new List<string>();

        ValidateProperty(failures, fieldNames.Code, nameof(fieldNames.Code));
        ValidateProperty(failures, fieldNames.Message, nameof(fieldNames.Message));
        ValidateProperty(failures, fieldNames.Status, nameof(fieldNames.Status));
        ValidateProperty(failures, fieldNames.FieldErrors, nameof(fieldNames.FieldErrors));
        ValidateProperty(failures, fieldNames.GlobalErrors, nameof(fieldNames.GlobalErrors));
        ValidateProperty(failures, fieldNames.ParameterErrors, nameof(fieldNames.ParameterErrors));
        ValidateProperty(failures, fieldNames.Property, nameof(fieldNames.Property));
        ValidateProperty(failures, fieldNames.RejectedValue, nameof(fieldNames.RejectedValue));
        ValidateProperty(failures, fieldNames.Path, nameof(fieldNames.Path));
        ValidateProperty(failures, fieldNames.Parameter, nameof(fieldNames.Parameter));
        ValidateProperty(failures, fieldNames.RetryAfter, nameof(fieldNames.RetryAfter));

        // Check for duplicate field names
        var allNames = new[]
        {
            fieldNames.Code, fieldNames.Message, fieldNames.Status,
            fieldNames.FieldErrors, fieldNames.GlobalErrors, fieldNames.ParameterErrors,
            fieldNames.Property, fieldNames.RejectedValue, fieldNames.Path, fieldNames.Parameter,
            fieldNames.RetryAfter
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fieldName in allNames)
        {
            if (fieldName != null && !seen.Add(fieldName))
            {
                failures.Add($"Duplicate JSON field name '{fieldName}' in JsonFieldNames configuration.");
            }
        }

        // Validate ProblemDetailTypePrefix (must be valid absolute URI or empty)
        if (!string.IsNullOrEmpty(options.ProblemDetailTypePrefix) &&
            !Uri.TryCreate(options.ProblemDetailTypePrefix, UriKind.Absolute, out _))
        {
            failures.Add($"ProblemDetailTypePrefix '{options.ProblemDetailTypePrefix}' is not a valid URI. Must be a valid absolute URI or empty string.");
        }

        // Validate RateLimiting.ErrorCode (must not be null or empty)
        if (string.IsNullOrWhiteSpace(options.RateLimiting.ErrorCode))
        {
            failures.Add("RateLimiting.ErrorCode must not be null or empty.");
        }

        // Validate RateLimiting.DefaultMessage (must not be null or empty)
        if (string.IsNullOrWhiteSpace(options.RateLimiting.DefaultMessage))
        {
            failures.Add("RateLimiting.DefaultMessage must not be null or empty.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateProperty(List<string> failures, string? value, string propertyName)
    {
        if (value is null)
            failures.Add($"JsonFieldNames.{propertyName} must not be null.");
        else if (string.IsNullOrWhiteSpace(value))
            failures.Add($"JsonFieldNames.{propertyName} must not be empty or whitespace.");
    }
}
