using FluentValidation;

namespace ShowcaseSample.Models;

/// <summary>
/// Request model validated via FluentValidation instead of DataAnnotations.
/// </summary>
public class CreateProductRequest
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Email { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// FluentValidation validator for CreateProductRequest.
/// Demonstrates automatic error code mapping to ErrorLens DefaultErrorCodes.
/// </summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()           // Maps to REQUIRED_NOT_EMPTY
            .MaximumLength(200);  // Maps to INVALID_SIZE

        RuleFor(x => x.Sku)
            .NotNull()            // Maps to REQUIRED_NOT_NULL
            .Matches(@"^[A-Z]{3}-\d{4}$")  // Maps to INVALID_PATTERN
            .WithMessage("SKU must be in format 'ABC-1234'");

        RuleFor(x => x.Price)
            .GreaterThan(0);      // Maps to INVALID_MIN

        RuleFor(x => x.Email)
            .EmailAddress()       // Maps to INVALID_EMAIL
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 10000)  // Maps to VALUE_OUT_OF_RANGE
            .WithErrorCode("QUANTITY_OUT_OF_RANGE");  // Custom code preserved as-is
    }
}
