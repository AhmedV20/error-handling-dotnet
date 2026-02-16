# Swashbuckle / Swagger Integration

ErrorLens provides automatic Swagger error response schema generation for pre-.NET 9 projects using Swashbuckle.

## Installation

```bash
dotnet add package ErrorLens.ErrorHandling.Swashbuckle
```

**Targets**: `net6.0`, `net7.0`, `net8.0`

## Setup

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingSwashbuckle();
```

This registers `ErrorResponseOperationFilter`, which implements `IOperationFilter`. It automatically adds error response schemas (400, 404, 500 by default) to all API operations.

## How It Works

- Adds error response schemas for each status code in `DefaultStatusCodes`
- Skips status codes already declared via `[ProducesResponseType]`
- Reflects `UseProblemDetailFormat` in generated schemas
- Respects custom `JsonFieldNamesOptions` for property naming
- Uses `ErrorResponseSchemaGenerator` internally (shared with OpenAPI package)

## Custom Status Codes

```csharp
builder.Services.AddErrorHandlingSwashbuckle(options =>
{
    options.DefaultStatusCodes = new HashSet<int> { 400, 401, 403, 422, 500 };
});
```

## Configuration

| Option | Default | Description |
|--------|---------|-------------|
| `DefaultStatusCodes` | `{ 400, 404, 500 }` | HTTP status codes to generate error schemas for |

## .NET 9+ Projects

For projects targeting .NET 9 or later, use the [OpenAPI package](/features/openapi) instead.
