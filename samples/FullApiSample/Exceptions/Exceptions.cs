using System.Net;
using ErrorLens.ErrorHandling.Attributes;

namespace FullApiSample.Exceptions;

/// <summary>
/// Exception thrown when a user is not found.
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
/// Exception thrown when email already exists.
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
/// Exception thrown when there are insufficient funds.
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
/// Base class for business exceptions.
/// </summary>
public abstract class BusinessException : Exception
{
    protected BusinessException(string message) : base(message) { }
}

/// <summary>
/// Exception for invalid order state transitions.
/// </summary>
[ResponseErrorCode("INVALID_ORDER_STATE")]
[ResponseStatus(HttpStatusCode.BadRequest)]
public class InvalidOrderStateException : BusinessException
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
