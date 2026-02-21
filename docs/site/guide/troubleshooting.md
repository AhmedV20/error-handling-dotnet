# Troubleshooting

Common issues and solutions when using ErrorLens.ErrorHandling.

## Rate Limiting

### Rate limiting returns plain text, not structured JSON

**Problem**: When rate limit is exceeded, you get plain text instead of ErrorLens JSON response.

**Solution**: You must wire up the `IRateLimitResponseWriter` in the `OnRejected` callback:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
    });

    // THIS IS REQUIRED for ErrorLens responses
    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, token);
    };
});
```

Without this callback, rate limit rejections return ASP.NET Core's default plain text response.

### [EnableRateLimiting] attribute not found

**Problem**: Compiler error when using `[EnableRateLimiting]` attribute.

**Solution**: Add the using directive:

```csharp
using Microsoft.AspNetCore.RateLimiting;
```

This attribute is from ASP.NET Core, not from ErrorLens.

### Rate limiting not working on Controllers

**Problem**: Controllers don't seem to be rate limited.

**Solutions**:

1. Ensure you called `app.UseRateLimiter()` in the middleware pipeline:

```csharp
app.UseErrorHandling();
app.UseRateLimiter();  // Must be called
app.MapControllers();
```

2. Verify the policy name matches:

```csharp
// Registration
options.AddFixedWindowLimiter("api", ...);

// Usage
[EnableRateLimiting("api")]  // Must match "api"
```

3. Check middleware order - `UseRateLimiter()` should come after `UseErrorHandling()` but before `MapControllers()`.

## Controllers

### Validation errors not using ErrorLens format

**Problem**: Validation errors return the default ASP.NET Core `ProblemDetails` format instead of ErrorLens's `fieldErrors` format.

**Solution**: Enable `OverrideModelStateValidation`:

```csharp
builder.Services.AddErrorHandling(options =>
{
    options.OverrideModelStateValidation = true;
});
```

By default, ASP.NET Core's `[ApiController]` attribute handles validation before ErrorLens can process it. This option allows ErrorLens to override that behavior.

### Custom exception handler not being called

**Problem**: Your `IApiExceptionHandler` implementation is not executing.

**Solutions**:

1. Ensure you registered it:

```csharp
builder.Services.AddApiExceptionHandler<MyCustomHandler>();
```

2. Check the `Order` property - lower values execute first:

```csharp
public class MyCustomHandler : IApiExceptionHandler
{
    public int Order => 50;  // Lower = higher priority

    public bool CanHandle(Exception exception)
        => exception is MyCustomException;

    public ApiErrorResponse Handle(Exception exception)
        => new ApiErrorResponse(...);
}
```

3. Verify `CanHandle()` returns true for your exception type:

```csharp
public bool CanHandle(Exception exception)
{
    // This must return true for your exception
    return exception is MyCustomException;
}
```

4. Check if a built-in handler is handling it first (built-in handlers have order 50-150). Use a lower order value to override:

```csharp
public int Order => 10;  // Execute before built-in handlers
```

## Configuration

### YAML configuration not loading

**Problem**: Settings in `errorhandling.yml` are not being applied.

**Solutions**:

1. Ensure you added the YAML configuration source:

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
```

2. Verify the file path is correct relative to the application root:

```csharp
// For files in the project root
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");

// For files in a subdirectory
builder.Configuration.AddYamlErrorHandling("config/errorhandling.yml");
```

3. Check the file is copied to output directory in your `.csproj`:

```xml
<ItemGroup>
  <Content Include="errorhandling.yml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

4. Bind configuration when registering ErrorLens:

```csharp
// Option 1: Pass IConfiguration directly
builder.Services.AddErrorHandling(builder.Configuration);

// Option 2: Bind explicitly
builder.Services.AddErrorHandling(options =>
    builder.Configuration.GetSection("ErrorHandling").Bind(options));
```

### appsettings.json configuration not working

**Problem**: Settings in `appsettings.json` are being ignored.

**Solution**: Ensure the section name matches exactly:

```json
{
  "ErrorHandling": {    // Must be "ErrorHandling" - case-sensitive
    "Enabled": true,
    "HttpStatusInJsonResponse": true,
    "OverrideModelStateValidation": true
  }
}
```

Then bind with:

```csharp
builder.Services.AddErrorHandling(builder.Configuration);
```

### Configuration priority not working as expected

**Problem**: Settings from one source override another unexpectedly.

**Understanding priority** (highest to lowest):

1. **Custom exception handlers** — `IApiExceptionHandler` implementations run first in the pipeline
2. **Inline options** — `Action<ErrorHandlingOptions>` in `AddErrorHandling()`
3. **Configuration binding** — `appsettings.json` or `errorhandling.yml`
4. **Exception attributes** — `[ResponseErrorCode]`, `[ResponseStatus]`
5. **Default conventions** — class name to `ALL_CAPS`, built-in HTTP status mappings

Code-based options always win. If you want YAML to take precedence, don't set the value in code:

```csharp
// This will always be "MY_CODE" regardless of YAML
builder.Services.AddErrorHandling(options =>
{
    options.RateLimiting.ErrorCode = "MY_CODE";  // This overrides YAML
});

// This allows YAML to set the value
builder.Services.AddErrorHandling();  // No explicit value = YAML wins
```

## Localization

### Localized messages not showing

**Problem**: Error messages are not being translated; showing default English instead.

**Solutions**:

1. Enable localization:

```csharp
builder.Services.AddErrorHandlingLocalization<Resources>();
```

2. Ensure resource files exist:
   - `Resources.resx` (default/fallback - English)
   - `Resources.fr.resx` (French)
   - `Resources.de.resx` (German)
   - etc.

3. Verify resource files are configured correctly:
   - **Build Action**: `Embedded Resource`
   - **Custom Tool**: `ResXFileCodeGenerator`
   - **Default Namespace**: Matches your project root

4. Add request localization middleware:

```csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "en", "fr", "de" });
    options.AddSupportedUICultures(new[] { "en", "fr", "de" });
    options.SetDefaultCulture("en");
});

var app = builder.Build();

app.UseRequestLocalization();  // Must be called
app.UseErrorHandling();
```

5. Ensure the client sends the `Accept-Language` header:

```bash
curl -H "Accept-Language: fr" http://localhost:5000/api/test
```

## OpenAPI/Swagger

### Error schemas not showing in Swagger

**Problem**: Swagger UI doesn't show error response schemas.

**For .NET 9+ (OpenAPI)**:

```csharp
builder.Services.AddOpenApi();
builder.Services.AddErrorHandlingOpenApi();  // This is required

var app = builder.Build();
app.MapOpenApi();  // This is required
```

**For .NET 6-8 (Swashbuckle)**:

```csharp
builder.Services.AddSwaggerGen();
builder.Services.AddErrorHandlingSwashbuckle();  // This is required

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
```

Without the `AddErrorHandlingOpenApi()` or `AddErrorHandlingSwashbuckle()` call, error schemas won't be added to your OpenAPI/Swagger documentation.

### Only some error codes show in schemas

**Problem**: Not all possible error codes appear in the OpenAPI schema.

**Solution**: By default, ErrorLens only adds schemas for common status codes (400, 404, 500). To customize:

```csharp
builder.Services.AddErrorHandlingOpenApi(options =>
{
    options.DefaultStatusCodes.Clear();
    options.DefaultStatusCodes.Add(400);  // Bad Request
    options.DefaultStatusCodes.Add(401);  // Unauthorized
    options.DefaultStatusCodes.Add(403);  // Forbidden
    options.DefaultStatusCodes.Add(404);  // Not Found
    options.DefaultStatusCodes.Add(409);  // Conflict
    options.DefaultStatusCodes.Add(422);  // Unprocessable Entity
    options.DefaultStatusCodes.Add(500);  // Internal Server Error
});
```

## General

### Middleware order issues

**Problem**: Error handling not working as expected.

**Solution**: Ensure correct middleware order:

```csharp
// CORRECT ORDER
app.UseErrorHandling();      // First - catch all errors
app.UseRateLimiter();        // After error handling
app.UseAuthentication();     // Standard ASP.NET middleware
app.UseAuthorization();
app.MapControllers();        // Endpoints last
```

**Why order matters**:
- `UseErrorHandling()` must be first to catch all exceptions
- `UseRateLimiter()` should come after so rate limit errors are handled
- Authentication/Authorization come before endpoints
- `MapControllers()` / `MapGet()` etc. should always be last

### Getting "IServiceCollection not found" errors

**Problem**: Build errors related to dependency injection.

**Solution**: Ensure you have the required using statements:

```csharp
using Microsoft.Extensions.DependencyInjection;
```

### Package conflicts

**Problem**: Build errors after installing ErrorLens packages.

**Solution**: Ensure you're using compatible versions:

| Package | .NET Version |
|---------|--------------|
| `ErrorLens.ErrorHandling` | 6.0, 7.0, 8.0, 9.0, 10.0 |
| `ErrorLens.ErrorHandling.OpenApi` | 9.0+ only |
| `ErrorLens.ErrorHandling.Swashbuckle` | 6.0, 7.0, 8.0 |

Don't install `ErrorLens.ErrorHandling.OpenApi` on .NET 8 or earlier - it's only for .NET 9+.

## Getting Help

If you're still stuck:

1. **Check the samples** - Working examples for all features:
   - [MinimalApiSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/MinimalApiSample) - Zero-config setup
   - [IntegrationSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/IntegrationSample) - OpenTelemetry, OpenAPI, Rate Limiting
   - [ShowcaseSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/ShowcaseSample) - All features with YAML config
   - [FullApiSample](https://github.com/AhmedV20/error-handling-dotnet/tree/main/samples/FullApiSample) - Controllers with custom handlers

2. **Review the API reference** - [API Reference](/reference/api)

3. **Check existing issues** - [GitHub Issues](https://github.com/AhmedV20/error-handling-dotnet/issues)

4. **Create a new issue** - Include:
   - .NET version
   - ErrorLens version
   - Code sample showing the problem
   - Expected vs actual behavior
