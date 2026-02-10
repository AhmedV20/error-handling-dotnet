using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Exceptions;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates configuration-driven exception handling.
/// These exceptions are mapped entirely via errorhandling.yml — no attributes needed.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigDrivenController : ControllerBase
{
    /// <summary>
    /// GET /api/configdriven/db-timeout → 503 DATABASE_TIMEOUT (configured via YAML)
    /// </summary>
    [HttpGet("db-timeout")]
    public IActionResult DatabaseTimeout()
    {
        throw new DatabaseTimeoutException("GetUserById");
    }

    /// <summary>
    /// GET /api/configdriven/service-down → 503 SERVICE_UNAVAILABLE (configured via YAML)
    /// </summary>
    [HttpGet("service-down")]
    public IActionResult ServiceDown()
    {
        throw new ServiceUnavailableException("PaymentGateway");
    }

    /// <summary>
    /// GET /api/configdriven/rate-limit → 429 RATE_LIMIT_EXCEEDED (configured via YAML)
    /// </summary>
    [HttpGet("rate-limit")]
    public IActionResult RateLimit()
    {
        throw new RateLimitExceededException(60);
    }
}
