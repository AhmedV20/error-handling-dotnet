# RFC 9457 Problem Details

ErrorLens supports [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) Problem Details response format as an opt-in feature.

## Enabling Problem Details

::: code-group
```yaml [YAML]
ErrorHandling:
  UseProblemDetailFormat: true
  ProblemDetailTypePrefix: https://api.example.com/errors/
  ProblemDetailConvertToKebabCase: true
```
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.UseProblemDetailFormat = true;
    options.ProblemDetailTypePrefix = "https://api.example.com/errors/";
});
```
:::

## Response Format

When enabled, responses use `application/problem+json` content type:

```json
{
  "type": "https://api.example.com/errors/user-not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "User abc-123 not found",
  "instance": "/api/users/abc-123",
  "code": "USER_NOT_FOUND"
}
```

### Field Descriptions

| Field | RFC 9457 | Description |
|-------|----------|-------------|
| `type` | Required | URI reference identifying the problem type |
| `title` | Required | Short human-readable summary (from HTTP status) |
| `status` | Required | HTTP status code |
| `detail` | Optional | Human-readable explanation (exception message) |
| `instance` | Optional | URI reference for the specific occurrence |
| `code` | Extension | Original error code from ErrorLens |

## Type URI Generation

| Error Code | Type URI |
|-----------|----------|
| `USER_NOT_FOUND` | `https://api.example.com/errors/user-not-found` |
| `VALIDATION_FAILED` | `https://api.example.com/errors/validation-failed` |
| `INTERNAL_ERROR` | `https://api.example.com/errors/internal-error` |

### Disabling Kebab-Case Conversion

```yaml
ErrorHandling:
  UseProblemDetailFormat: true
  ProblemDetailConvertToKebabCase: false
```

Result: `"type": "USER_NOT_FOUND"` instead of `"type": "https://...errors/user-not-found"`

## Validation Errors with Problem Detail

```json
{
  "type": "https://api.example.com/errors/validation-failed",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "fieldErrors": [
    {
      "code": "REQUIRED_NOT_NULL",
      "property": "email",
      "message": "Email is required",
      "path": "email"
    }
  ]
}
```

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `UseProblemDetailFormat` | `false` | Enable Problem Details format |
| `ProblemDetailTypePrefix` | `https://example.com/errors/` | URI prefix for type field |
| `ProblemDetailConvertToKebabCase` | `true` | Convert error codes to kebab-case |

## Compatibility

Problem Details format works with all other features:

- Custom exception attributes still apply
- Response customizers add properties as extensions
- Configuration-based code/message overrides still work
- Custom handlers produce responses that get converted to Problem Details
