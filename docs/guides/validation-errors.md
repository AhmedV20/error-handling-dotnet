# Validation Errors

ErrorLens.ErrorHandling automatically handles ASP.NET Core model validation errors, producing detailed field-level error information.

## How It Works

When ASP.NET Core model validation fails (via `[ApiController]` and DataAnnotations), the library intercepts the `BadHttpRequestException` and produces a structured response with `fieldErrors`.

## Example

### Model

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }
}
```

### Request

```http
POST /api/users
Content-Type: application/json

{
  "name": "A",
  "email": "not-an-email",
  "age": 5
}
```

### Response (HTTP 400)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "STRING_LENGTH",
      "property": "Name",
      "message": "The field Name must be a string with a minimum length of 2 and a maximum length of 100.",
      "rejectedValue": "A",
      "path": "Name"
    },
    {
      "code": "EMAIL_ADDRESS",
      "property": "Email",
      "message": "Invalid email format",
      "rejectedValue": "not-an-email",
      "path": "Email"
    },
    {
      "code": "RANGE",
      "property": "Age",
      "message": "Age must be between 18 and 120",
      "rejectedValue": 5,
      "path": "Age"
    }
  ]
}
```

## Field Error Structure

Each field error contains:

| Field | Type | Description |
|-------|------|-------------|
| `code` | `string` | Validation error code (derived from validation attribute) |
| `property` | `string` | Property name that failed validation |
| `message` | `string` | Human-readable error message |
| `rejectedValue` | `any?` | The value that failed validation (null if not available) |
| `path` | `string?` | Full property path for nested objects (configurable via `AddPathToError`) |

## Custom Validation Error Codes

Override validation error codes in configuration:

```yaml
Codes:
  email.Required: EMAIL_IS_REQUIRED
  email.EmailAddress: EMAIL_FORMAT_INVALID
  name.StringLength: NAME_LENGTH_INVALID
```

The key format is `{propertyName}.{validationType}`.

## Custom Validation Error Messages

```yaml
Messages:
  email.Required: A valid email address is required
  email.EmailAddress: Please provide a properly formatted email address
```

## Global Errors

For cross-field or class-level validation errors:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "globalErrors": [
    {
      "code": "PASSWORDS_MUST_MATCH",
      "message": "Password and confirmation password must match"
    }
  ]
}
```

## Parameter Errors

For method parameter validation errors:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "parameterErrors": [
    {
      "code": "REQUIRED",
      "parameter": "id",
      "message": "Id is required",
      "rejectedValue": null
    }
  ]
}
```
