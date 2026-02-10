using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Exceptions;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates RFC 9457 Problem Details format.
/// Enable UseProblemDetailFormat in errorhandling.yml to see application/problem+json responses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProblemDetailsController : ControllerBase
{
    /// <summary>
    /// GET /api/problemdetails/not-found → RFC 9457 Problem Details format
    /// Response includes: type, title, status, detail, instance, extensions
    /// </summary>
    [HttpGet("not-found")]
    public new IActionResult NotFound()
    {
        throw new UserNotFoundException("user-456");
    }

    /// <summary>
    /// GET /api/problemdetails/server-error → RFC 9457 with 500 status
    /// </summary>
    [HttpGet("server-error")]
    public IActionResult ServerError()
    {
        throw new Exception("An unexpected internal error occurred");
    }

    /// <summary>
    /// GET /api/problemdetails/forbidden → RFC 9457 with 403 status
    /// </summary>
    [HttpGet("forbidden")]
    public IActionResult Forbidden()
    {
        throw new ForbiddenException("SuperAdmin");
    }
}
