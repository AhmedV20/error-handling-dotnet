# Custom JSON Field Names

Rename any JSON property in error responses to match your API conventions.

## Configuration

::: code-group
```yaml [YAML]
ErrorHandling:
  JsonFieldNames:
    Code: type
    Message: detail
    Status: statusCode
    FieldErrors: fields
    GlobalErrors: errors
    ParameterErrors: params
    Property: field
    RejectedValue: value
    Path: jsonPath
    Parameter: param
    RetryAfter: retry_after
```
```json [JSON]
{
  "ErrorHandling": {
    "JsonFieldNames": {
      "Code": "type",
      "Message": "detail",
      "Status": "statusCode",
      "FieldErrors": "fields"
    }
  }
}
```
```csharp [Code]
builder.Services.AddErrorHandling(options =>
{
    options.JsonFieldNames.Code = "type";
    options.JsonFieldNames.Message = "detail";
});
```
:::

## Available Field Names

### Top-Level Response

| Option | Default | Description |
|--------|---------|-------------|
| `Code` | `code` | Error code field |
| `Message` | `message` | Error message field |
| `Status` | `status` | HTTP status code field |
| `FieldErrors` | `fieldErrors` | Field errors array |
| `GlobalErrors` | `globalErrors` | Global errors array |
| `ParameterErrors` | `parameterErrors` | Parameter errors array |

### Nested Error Objects

| Option | Default | Used In | Description |
|--------|---------|---------|-------------|
| `Property` | `property` | Field errors | Property name |
| `RejectedValue` | `rejectedValue` | Field/parameter errors | Rejected value |
| `Path` | `path` | Field errors | Property path |
| `Parameter` | `parameter` | Parameter errors | Parameter name |
| `RetryAfter` | `retryAfter` | Rate limit responses | Retry-after seconds |

::: tip
`Code` and `Message` are shared — they apply to both the top-level response and nested error objects.
:::

## Example

### Default Output

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
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

### With Custom Names

```json
{
  "type": "VALIDATION_FAILED",
  "detail": "Validation failed",
  "statusCode": 400,
  "fields": [
    {
      "type": "REQUIRED_NOT_NULL",
      "field": "email",
      "detail": "Email is required",
      "jsonPath": "email"
    }
  ]
}
```

## How It Works

ErrorLens uses a custom `JsonConverter<ApiErrorResponse>` that reads the configured field names at serialization time:

- `[JsonPropertyName]` attributes on model classes are bypassed
- Field names are determined at runtime from configuration
- Changes don't require code changes or recompilation
- All serialization paths use the same converter
