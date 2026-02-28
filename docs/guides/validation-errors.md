# Validation Errors

ErrorLens.ErrorHandling can handle ASP.NET Core model validation errors, producing detailed field-level error information.

## Enabling Unified Validation Format

By default, `[ApiController]` automatic model validation uses ASP.NET Core's built-in `ProblemDetails` response. To use ErrorLens's structured `fieldErrors` format instead, enable the `OverrideModelStateValidation` option:

### YAML

```yaml
ErrorHandling:
  OverrideModelStateValidation: true
```

### JSON

```json
{
  "ErrorHandling": {
    "OverrideModelStateValidation": true
  }
}
```

### Code

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

When `false` (default), ASP.NET Core's built-in validation response is preserved — ErrorLens only handles thrown exceptions. When `true`, validation errors are intercepted and returned using ErrorLens's structured format with `fieldErrors`, `globalErrors`, and custom JSON field names.

## How It Works

When `OverrideModelStateValidation` is enabled and ASP.NET Core model validation fails (via `[ApiController]` and DataAnnotations), the library intercepts the `ModelStateDictionary` and produces a structured response with `fieldErrors`.

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
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "INVALID_SIZE",
      "property": "name",
      "message": "The field Name must be a string with a minimum length of 2 and a maximum length of 100.",
      "path": "name"
    },
    {
      "code": "INVALID_EMAIL",
      "property": "email",
      "message": "Invalid email format",
      "path": "email"
    },
    {
      "code": "VALUE_OUT_OF_RANGE",
      "property": "age",
      "message": "Age must be between 18 and 120",
      "path": "age"
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
  "code": "VALIDATION_FAILED",
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
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "parameterErrors": [
    {
      "code": "REQUIRED_NOT_NULL",
      "parameter": "id",
      "message": "Id is required",
      "rejectedValue": null
    }
  ]
}
```

## FluentValidation Integration

ErrorLens supports [FluentValidation](https://docs.fluentvalidation.net/) as an alternative validation source alongside DataAnnotations. The companion package automatically catches `FluentValidation.ValidationException` and maps validation failures to the same structured `ApiErrorResponse` format with `fieldErrors`.

### Installation

```bash
dotnet add package ErrorLens.ErrorHandling.FluentValidation
```

### Registration

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingFluentValidation();
```

### Comparison: DataAnnotations vs FluentValidation

Both validation sources produce equivalent structured responses.

**DataAnnotations**

```csharp
public class CreateUserRequest
{
    [Required]
    public string? Name { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
```

**FluentValidation**

```csharp
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

**Both produce the same response structure (HTTP 400)**

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "fieldErrors": [
    {
      "code": "REQUIRED_NOT_EMPTY",
      "property": "name",
      "message": "'Name' must not be empty.",
      "rejectedValue": null,
      "path": "name"
    },
    {
      "code": "INVALID_EMAIL",
      "property": "email",
      "message": "'Email' is not a valid email address.",
      "rejectedValue": "not-an-email",
      "path": "email"
    }
  ]
}
```

### Error Code Mapping

FluentValidation validators are automatically mapped to ErrorLens error codes:

| FluentValidation Validator | ErrorLens Code |
|----------------------------|----------------|
| `NotEmptyValidator` | `REQUIRED_NOT_EMPTY` |
| `NotNullValidator` | `REQUIRED_NOT_NULL` |
| `EmailValidator` | `INVALID_EMAIL` |
| `LengthValidator` | `INVALID_SIZE` |
| `GreaterThanValidator` | `VALUE_TOO_LOW` |
| `LessThanValidator` | `VALUE_TOO_HIGH` |
| `RegularExpressionValidator` | `REGEX_PATTERN_VALIDATION_FAILED` |
| `InclusiveBetweenValidator` | `VALUE_OUT_OF_RANGE` |

> **Note:** This table shows common mappings. The full list includes 14 built-in validator mappings — see [the AsciiDoc reference](../index.adoc) for the complete table, including `MaximumLengthValidator`, `MinimumLengthValidator`, `CreditCardValidator`, `ExclusiveBetweenValidator`, `LessThanOrEqualValidator`, and `GreaterThanOrEqualValidator`.

Custom codes set via `.WithErrorCode()` are preserved as-is:

```csharp
RuleFor(x => x.Name).NotEmpty().WithErrorCode("CUSTOM_NAME_REQUIRED");
```

### Severity Filtering

By default, only `Severity.Error` failures are included in the response. To include `Warning` or `Info` severities:

```csharp
builder.Services.AddErrorHandlingFluentValidation(options =>
{
    options.IncludeSeverities.Add(FluentValidation.Severity.Warning);
    options.IncludeSeverities.Add(FluentValidation.Severity.Info);
});
```

### Nested Properties

Nested property names are camelCased segment-by-segment. For example, `Address.City.ZipCode` becomes `address.city.zipCode` in both the `property` and `path` fields.

### Custom Validation Message

The top-level `"Validation failed"` message can be customized via `BuiltInMessages`:

```yaml
ErrorHandling:
  BuiltInMessages:
    VALIDATION_FAILED: "One or more fields are invalid"
```
