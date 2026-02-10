using System.Net;
using ErrorLens.ErrorHandling.Attributes;

namespace ShowcaseSample.Exceptions;

// --- Domain Exceptions ---

/// <summary>
/// Thrown when a requested user cannot be found.
/// Demonstrates: [ResponseErrorCode], [ResponseStatus], [ResponseErrorProperty]
/// </summary>
[ResponseErrorCode("USER_NOT_FOUND")]
[ResponseStatus(HttpStatusCode.NotFound)]
public class UserNotFoundException : Exception
{
    [ResponseErrorProperty("userId")]
    public string UserId { get; }

    public UserNotFoundException(string userId)
        : base($"User with ID '{userId}' was not found")
    {
        UserId = userId;
    }
}

/// <summary>
/// Thrown when attempting to create a user with an existing email.
/// Demonstrates: [ResponseErrorCode], [ResponseStatus], [ResponseErrorProperty]
/// </summary>
[ResponseErrorCode("EMAIL_ALREADY_EXISTS")]
[ResponseStatus(HttpStatusCode.Conflict)]
public class DuplicateEmailException : Exception
{
    [ResponseErrorProperty("email")]
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base($"A user with email '{email}' already exists")
    {
        Email = email;
    }
}

/// <summary>
/// Thrown when a financial operation lacks sufficient funds.
/// Demonstrates: multiple [ResponseErrorProperty] attributes.
/// </summary>
[ResponseErrorCode("INSUFFICIENT_FUNDS")]
[ResponseStatus(HttpStatusCode.UnprocessableEntity)]
public class InsufficientFundsException : Exception
{
    [ResponseErrorProperty("required")]
    public decimal RequiredAmount { get; }

    [ResponseErrorProperty("available")]
    public decimal AvailableAmount { get; }

    public InsufficientFundsException(decimal required, decimal available)
        : base($"Insufficient funds: required {required:C}, available {available:C}")
    {
        RequiredAmount = required;
        AvailableAmount = available;
    }
}

/// <summary>
/// Thrown when an order state transition is invalid.
/// </summary>
[ResponseErrorCode("INVALID_ORDER_STATE")]
[ResponseStatus(HttpStatusCode.BadRequest)]
public class InvalidOrderStateException : Exception
{
    [ResponseErrorProperty("currentState")]
    public string CurrentState { get; }

    [ResponseErrorProperty("requestedState")]
    public string RequestedState { get; }

    public InvalidOrderStateException(string currentState, string requestedState)
        : base($"Cannot transition order from '{currentState}' to '{requestedState}'")
    {
        CurrentState = currentState;
        RequestedState = requestedState;
    }
}
