namespace ShowcaseSample.Exceptions;

/// <summary>
/// Custom exception to demonstrate FullQualifiedName error code strategy.
/// When DefaultErrorCodeStrategy is set to FullQualifiedName, this will generate
/// error code like "ShowcaseSample.Exceptions.InsufficientInventoryException".
/// </summary>
public class InsufficientInventoryException : Exception
{
    public string ProductId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientInventoryException(string productId, int requested, int available)
        : base($"Insufficient inventory for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }
}

/// <summary>
/// Another custom exception for demonstrating FullQualifiedName.
/// </summary>
public class PaymentDeclinedException : Exception
{
    public string Reason { get; }

    public PaymentDeclinedException(string reason)
        : base($"Payment declined: {reason}")
    {
        Reason = reason;
    }
}
