using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Exceptions;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates attribute-based exception customization.
/// Exceptions use [ResponseErrorCode], [ResponseStatus], [ResponseErrorProperty].
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AttributesController : ControllerBase
{
    /// <summary>
    /// GET /api/attributes/user/123 → 404 USER_NOT_FOUND with userId property
    /// </summary>
    [HttpGet("user/{id}")]
    public IActionResult GetUser(string id)
    {
        throw new UserNotFoundException(id);
    }

    /// <summary>
    /// POST /api/attributes/register → 409 EMAIL_ALREADY_EXISTS with email property
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromQuery] string email = "john@example.com")
    {
        throw new DuplicateEmailException(email);
    }

    /// <summary>
    /// POST /api/attributes/transfer → 422 INSUFFICIENT_FUNDS with required + available properties
    /// </summary>
    [HttpPost("transfer")]
    public IActionResult Transfer()
    {
        throw new InsufficientFundsException(500.00m, 123.45m);
    }

    /// <summary>
    /// POST /api/attributes/order/ship → 400 INVALID_ORDER_STATE with currentState + requestedState
    /// </summary>
    [HttpPost("order/ship")]
    public IActionResult ShipOrder()
    {
        throw new InvalidOrderStateException("Draft", "Shipped");
    }

    /// <summary>
    /// GET /api/attributes/forbidden → 403 FORBIDDEN with requiredRole property
    /// </summary>
    [HttpGet("forbidden")]
    public IActionResult Forbidden()
    {
        throw new ForbiddenException("Admin");
    }

    /// <summary>
    /// GET /api/attributes/unauthorized → 401 UNAUTHORIZED
    /// </summary>
    [HttpGet("unauthorized")]
    public new IActionResult Unauthorized()
    {
        throw new UnauthorizedException();
    }
}
