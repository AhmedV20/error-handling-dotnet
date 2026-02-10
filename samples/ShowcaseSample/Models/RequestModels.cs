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
