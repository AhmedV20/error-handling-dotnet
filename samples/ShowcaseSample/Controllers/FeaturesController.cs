using Microsoft.AspNetCore.Mvc;
using ShowcaseSample.Exceptions;

namespace ShowcaseSample.Controllers;

/// <summary>
/// Demonstrates additional ErrorLens features not shown in other controllers.
///
/// FullQualifiedName Error Code Strategy:
///   By default, the project uses AllCaps strategy (e.g., "INSUFFICIENT_INVENTORY").
///   To see the FullQualifiedName strategy in action, change errorhandling.yml:
///     DefaultErrorCodeStrategy: FullQualifiedName
///   This produces error codes like "ShowcaseSample.Exceptions.InsufficientInventoryException".
///
/// These endpoints work with either strategy — the difference is only in the error code format.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    /// <summary>
    /// GET /api/features/inventory-error - Demonstrates error code strategy behavior.
    ///
    /// With AllCaps (default): { "type": "INSUFFICIENT_INVENTORY", ... }
    /// With FullQualifiedName:  { "type": "ShowcaseSample.Exceptions.InsufficientInventoryException", ... }
    /// </summary>
    [HttpGet("inventory-error")]
    public IActionResult InventoryError()
    {
        throw new InsufficientInventoryException("PROD-123", 10, 5);
    }

    /// <summary>
    /// GET /api/features/payment-declined - Another example showing strategy differences.
    ///
    /// With AllCaps (default): { "type": "PAYMENT_DECLINED", ... }
    /// With FullQualifiedName:  { "type": "ShowcaseSample.Exceptions.PaymentDeclinedException", ... }
    /// </summary>
    [HttpGet("payment-declined")]
    public IActionResult PaymentDeclined()
    {
        throw new PaymentDeclinedException("Insufficient funds");
    }
}
