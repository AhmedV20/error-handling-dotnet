using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Models;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates validation error handling with DataAnnotations.
/// Invalid requests produce detailed fieldErrors arrays.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    /// <summary>
    /// POST /api/validation/users — Submit invalid data to see fieldErrors.
    /// Try: {} or {"name": "A", "email": "bad", "age": 5}
    /// </summary>
    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        // If we reach here, validation passed
        return Ok(new { message = "User created", name = request.Name, email = request.Email });
    }

    /// <summary>
    /// POST /api/validation/transfer — Submit invalid transfer to see fieldErrors.
    /// Try: {"amount": -5}
    /// </summary>
    [HttpPost("transfer")]
    public IActionResult Transfer([FromBody] TransferRequest request)
    {
        return Ok(new { message = "Transfer initiated", amount = request.Amount });
    }
}
