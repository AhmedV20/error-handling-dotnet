# OpenAPI Integration (.NET 9+)

ErrorLens provides automatic OpenAPI error response schema generation for .NET 9+ projects using the built-in OpenAPI support.

## Installation

```bash
dotnet add package ErrorLens.ErrorHandling.OpenApi
```

**Targets**: `net9.0`, `net10.0`

## Setup

```csharp
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingOpenApi();
```

This registers `ErrorResponseOperationTransformer`, which implements `IOpenApiOperationTransformer`. It automatically adds error response schemas (400, 404, 500 by default) to all API operations.

## How It Works

- Adds error response schemas for each status code in `DefaultStatusCodes`
- Skips status codes already declared via `[ProducesResponseType]` attributes
- Reflects `UseProblemDetailFormat` in the generated schemas (standard vs. Problem Details)
- Respects custom `JsonFieldNamesOptions` for property naming
- Uses `ErrorResponseSchemaGenerator` internally (shared with the Swashbuckle package)

## Custom Status Codes

Override the default set of status codes:

```csharp
builder.Services.AddErrorHandlingOpenApi(options =>
{
    options.DefaultStatusCodes = new HashSet<int> { 400, 401, 422, 500 };
});
```

## Configuration

`OpenApiOptions` is configured via the setup delegate:

| Option | Default | Description |
|--------|---------|-------------|
| `DefaultStatusCodes` | `{ 400, 404, 500 }` | HTTP status codes to generate error schemas for |

## For Pre-.NET 9 Projects

If your project targets .NET 6, .NET 7, or .NET 8 and uses Swashbuckle, use the [Swashbuckle package](swashbuckle.md) instead.
