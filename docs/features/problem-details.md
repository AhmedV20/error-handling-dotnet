# RFC 9457 Problem Details

ErrorLens.ErrorHandling supports RFC 9457 (formerly RFC 7807) Problem Details response format as an opt-in feature.

## Enabling Problem Details

### Via Configuration

```yaml
ErrorHandling:
  UseProblemDetailFormat: true
  ProblemDetailTypePrefix: https://api.example.com/errors/
  ProblemDetailConvertToKebabCase: true
```

### Via Code

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.UseProblemDetailFormat = true;
    options.ProblemDetailTypePrefix = "https://api.example.com/errors/";
});
```

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
| `instance` | Optional | URI reference for the specific occurrence (request path) |
| `code` | Extension | Original error code from ErrorLens |

## Type URI Generation

The `type` field is generated from the error code:

1. Start with `ProblemDetailTypePrefix` (default: `https://example.com/errors/`)
2. Convert error code to kebab-case (if `ProblemDetailConvertToKebabCase` is true)
3. Append to prefix

| Error Code | Type URI |
|-----------|----------|
| `USER_NOT_FOUND` | `https://api.example.com/errors/user-not-found` |
| `VALIDATION_ERROR` | `https://api.example.com/errors/validation-error` |
| `INTERNAL_ERROR` | `https://api.example.com/errors/internal-error` |

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `UseProblemDetailFormat` | `false` | Enable Problem Details format |
| `ProblemDetailTypePrefix` | `https://example.com/errors/` | URI prefix for type field |
| `ProblemDetailConvertToKebabCase` | `true` | Convert error codes to kebab-case |

## Compatibility

Problem Details format works with all other features:

- Custom exception attributes still apply
- Response customizers still add properties (as extensions)
- Configuration-based code/message overrides still work
- Custom handlers produce responses that get converted to Problem Details
