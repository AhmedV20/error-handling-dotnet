using FluentValidation;

namespace ErrorLens.ErrorHandling.FluentValidation;

/// <summary>
/// Configuration options for the FluentValidation exception handler.
/// </summary>
public class FluentValidationOptions
{
    /// <summary>
    /// Which FluentValidation severity levels to include in error responses.
    /// By default, only <see cref="Severity.Error"/> failures are mapped.
    /// Add <see cref="Severity.Warning"/> and/or <see cref="Severity.Info"/> to include those as well.
    /// </summary>
    public HashSet<Severity> IncludeSeverities { get; set; } = new() { Severity.Error };
}
