# Exception Attributes

Decorate exception classes with attributes to control error responses without configuration files.

## ResponseErrorCode

Sets a custom error code for an exception type:

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId)
        : base($"User {userId} not found") { }
}
```

Response:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "User abc-123 not found"
}
```

Without the attribute, the code is auto-generated using the configured `DefaultErrorCodeStrategy`:

| Strategy | Result |
|----------|--------|
| `AllCaps` (default) | `USER_NOT_FOUND` |
| `FullQualifiedName` | `MyApp.Exceptions.UserNotFoundException` |

## ResponseStatus

Sets the HTTP status code for an exception type:

```csharp
[ResponseErrorCode("USER_NOT_FOUND")]
[ResponseStatus(HttpStatusCode.NotFound)]
public class UserNotFoundException : Exception { ... }
```

Without the attribute, the default HTTP status mapping applies (e.g., 500 for unknown exceptions).

## ResponseErrorProperty

Exposes exception properties as additional fields in the error response:

```csharp
[ResponseErrorCode("INSUFFICIENT_FUNDS")]
[ResponseStatus(HttpStatusCode.UnprocessableEntity)]
public class InsufficientFundsException : Exception
{
    [ResponseErrorProperty("required")]
    public decimal RequiredAmount { get; }

    [ResponseErrorProperty("available")]
    public decimal AvailableAmount { get; }

    public InsufficientFundsException(decimal required, decimal available)
        : base("Insufficient funds")
    {
        RequiredAmount = required;
        AvailableAmount = available;
    }
}
```

Response:

```json
{
  "code": "INSUFFICIENT_FUNDS",
  "message": "Insufficient funds",
  "required": 500.00,
  "available": 123.45
}
```

### Name Parameter

```csharp
[ResponseErrorProperty("user_id")]  // JSON key: "user_id"
public string UserId { get; }

[ResponseErrorProperty]             // JSON key: property name (camelCase)
public string Email { get; }
```

### IncludeIfNull

By default, null properties are omitted. Set `IncludeIfNull = true` to always include them:

```csharp
[ResponseErrorProperty("details", IncludeIfNull = true)]
public string? Details { get; }
```

## Combining Attributes

All three attributes can be combined on a single exception class:

```csharp
[ResponseErrorCode("INVALID_ORDER_STATE")]
[ResponseStatus(HttpStatusCode.BadRequest)]
public class InvalidOrderStateException : Exception
{
    [ResponseErrorProperty("currentState")]
    public string CurrentState { get; }

    [ResponseErrorProperty("requestedState")]
    public string RequestedState { get; }

    public InvalidOrderStateException(string current, string requested)
        : base($"Cannot transition from '{current}' to '{requested}'")
    {
        CurrentState = current;
        RequestedState = requested;
    }
}
```

## Priority vs Configuration

Configuration-based mappings take precedence over attributes:

1. Configuration (`Codes`, `HttpStatuses` in appsettings/YAML)
2. Attributes (`[ResponseErrorCode]`, `[ResponseStatus]`)
3. Default conventions (class name to ALL_CAPS)
