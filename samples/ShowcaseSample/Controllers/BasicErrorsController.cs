using Microsoft.AspNetCore.Mvc;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates zero-config exception handling.
/// No attributes, no configuration — just throw exceptions and get structured JSON responses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BasicErrorsController : ControllerBase
{
    /// <summary>
    /// GET /api/basicerrors/generic → 500 INTERNAL_ERROR
    /// </summary>
    [HttpGet("generic")]
    public IActionResult GenericException()
    {
        throw new Exception("Something unexpected happened");
    }

    /// <summary>
    /// GET /api/basicerrors/invalid-operation → 400 INVALID_OPERATION
    /// </summary>
    [HttpGet("invalid-operation")]
    public IActionResult InvalidOperation()
    {
        throw new InvalidOperationException("Cannot perform this operation in the current state");
    }

    /// <summary>
    /// GET /api/basicerrors/argument-null → 400 ARGUMENT_NULL
    /// </summary>
    [HttpGet("argument-null")]
    public IActionResult ArgumentNull()
    {
        throw new ArgumentNullException("userId", "User ID cannot be null");
    }

    /// <summary>
    /// GET /api/basicerrors/argument → 400 ARGUMENT
    /// </summary>
    [HttpGet("argument")]
    public IActionResult Argument()
    {
        throw new ArgumentException("Invalid format for user ID", "userId");
    }

    /// <summary>
    /// GET /api/basicerrors/key-not-found → 404 KEY_NOT_FOUND
    /// </summary>
    [HttpGet("key-not-found")]
    public IActionResult KeyNotFound()
    {
        throw new KeyNotFoundException("The requested resource was not found");
    }

    /// <summary>
    /// GET /api/basicerrors/timeout → 500 TIMEOUT
    /// </summary>
    [HttpGet("timeout")]
    public IActionResult Timeout()
    {
        throw new TimeoutException("The operation timed out after 30 seconds");
    }

    /// <summary>
    /// GET /api/basicerrors/not-implemented → 500 NOT_IMPLEMENTED
    /// </summary>
    [HttpGet("not-implemented")]
    public IActionResult NotImplementedAction()
    {
        throw new NotImplementedException("This feature is coming soon");
    }

    /// <summary>
    /// GET /api/basicerrors/unauthorized → 401 UNAUTHORIZED_ACCESS
    /// </summary>
    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedAccess()
    {
        throw new UnauthorizedAccessException("You must be logged in");
    }
}
