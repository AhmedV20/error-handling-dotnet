using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Models;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates FluentValidation integration with ErrorLens.
/// FluentValidation errors are automatically caught and returned as structured fieldErrors.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FluentValidationController : ControllerBase
{
    /// <summary>
    /// POST /api/fluentvalidation/products — Validates using FluentValidation.
    /// Try: {"name": "", "sku": "bad", "price": -5, "email": "invalid", "quantity": 0}
    /// The response uses the same fieldErrors format as DataAnnotations validation.
    /// </summary>
    [HttpPost("products")]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        var validator = new CreateProductRequestValidator();
        var result = validator.Validate(request);

        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return Ok(new { message = "Product created", name = request.Name, sku = request.Sku });
    }
}
