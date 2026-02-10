# Custom JSON Field Names

Rename any JSON property in error responses to match your API conventions or frontend expectations.

## Configuration

### YAML

```yaml
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
    Parameter: paramName
```

### JSON

```json
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

### Code

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.JsonFieldNames.Code = "type";
    options.JsonFieldNames.Message = "detail";
});
```

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

Note: `Code` and `Message` are shared — they apply to both the top-level response and nested error objects.

## Examples

### Default Output

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "status": 400,
  "fieldErrors": [
    {
      "code": "REQUIRED",
      "property": "email",
      "message": "Email is required",
      "path": "email"
    }
  ]
}
```

### With Custom Names

Configuration:
```yaml
JsonFieldNames:
  Code: type
  Message: detail
  Status: statusCode
  FieldErrors: fields
  Property: field
  Path: jsonPath
```

Output:
```json
{
  "type": "VALIDATION_ERROR",
  "detail": "Validation failed",
  "statusCode": 400,
  "fields": [
    {
      "type": "REQUIRED",
      "field": "email",
      "detail": "Email is required",
      "jsonPath": "email"
    }
  ]
}
```

## How It Works

ErrorLens.ErrorHandling uses a custom `JsonConverter<ApiErrorResponse>` that reads the configured field names at serialization time. This means:

- The `[JsonPropertyName]` attributes on model classes are bypassed
- Field names are determined at runtime from configuration
- Changes to field names don't require code changes or recompilation
- All serialization paths (middleware and IExceptionHandler) use the same converter
