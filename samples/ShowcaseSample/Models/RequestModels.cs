using System.ComponentModel.DataAnnotations;

namespace ShowcaseSample.Models;

/// <summary>
/// Request model for creating a user. Uses DataAnnotations for validation.
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters")]
    public string? Password { get; set; }
}

/// <summary>
/// Request model for transferring funds.
/// </summary>
public class TransferRequest
{
    [Required]
    public string? FromAccount { get; set; }

    [Required]
    public string? ToAccount { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
}

/// <summary>
/// Request model for changing password. Demonstrates global errors via IValidatableObject.
/// Global errors are cross-field validation errors that don't apply to a single property.
/// </summary>
public class ChangePasswordRequest : IValidatableObject
{
    [Required(ErrorMessage = "Current password is required")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirmation password is required")]
    public string? ConfirmPassword { get; set; }

    /// <summary>
    /// Validates cross-field rules. This produces globalErrors instead of fieldErrors.
    /// IMPORTANT: To create true globalErrors, pass null or empty array for member names.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Global error: passwords must match (null member names = global error)
        if (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword) && NewPassword != ConfirmPassword)
        {
            results.Add(new ValidationResult(
                "New password and confirmation password must match.",
                null  // No member names = global error
            ));
        }

        // Global error: new password must be different from current
        if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword) && CurrentPassword == NewPassword)
        {
            results.Add(new ValidationResult(
                "New password must be different from current password.",
                null  // No member names = global error
            ));
        }

        // Global error: password cannot contain "admin"
        if (!string.IsNullOrEmpty(NewPassword) && NewPassword.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new ValidationResult(
                "Password cannot contain the word 'admin'.",
                null  // No member names = global error
            ));
        }

        return results;
    }
}
