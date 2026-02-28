using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.OpenApi;
using ErrorLens.ErrorHandling.RateLimiting;
using FluentValidation;
using IntegrationSample;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- Error Handling ---
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = true;
    options.OverrideModelStateValidation = true;
    options.IncludeRejectedValues = true;

    // v1.4.0: Use kebab-case error code strategy
    options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

    // v1.4.0: Custom 5xx fallback message
    options.FallbackMessage = "An internal error occurred. Please try again or contact support.";

    // Configure rate limiting responses
    options.RateLimiting = new()
    {
        ErrorCode = "RATE_LIMIT_EXCEEDED",
        DefaultMessage = "Too many requests. Please try again later.",
        IncludeRetryAfterInBody = true,
        UseModernHeaderFormat = true  // Use IETF draft RateLimit-* headers
    };
});

// --- FluentValidation Integration (v1.4.0) ---
builder.Services.AddErrorHandlingFluentValidation();

// --- Localization ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddErrorHandlingLocalization<ErrorMessages>();

// Configure request localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "fr", "es" };
    options.SetDefaultCulture(supportedCultures[0]);
    options.AddSupportedUICultures(supportedCultures);
    options.FallBackToParentUICultures = true;
});

// --- OpenTelemetry Tracing ---
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ErrorLens.ErrorHandling")
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

// --- OpenAPI Integration (.NET 9+) ---
builder.Services.AddOpenApi();
builder.Services.AddErrorHandlingOpenApi();

// --- Rate Limiting (.NET 7+) ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
    });

    options.OnRejected = async (context, token) =>
    {
        var writer = context.HttpContext.RequestServices
            .GetRequiredService<IRateLimitResponseWriter>();
        await writer.WriteRateLimitResponseAsync(
            context.HttpContext, context.Lease, token);
    };
});

var app = builder.Build();

// Middleware pipeline order matters:
// 1. RequestLocalization — sets culture before error messages are generated
// 2. ErrorHandling — catches exceptions and returns structured responses
// 3. RateLimiter — returns 429 via IRateLimitResponseWriter
app.UseRequestLocalization();
app.UseErrorHandling();
app.UseRateLimiter();
app.MapOpenApi();

// --- Demo Endpoints ---

// Basic error — demonstrates default error handling
app.MapGet("/error", () =>
{
    throw new InvalidOperationException("This is a demo error");
})
.WithTags("Errors");

// Not found — demonstrates built-in 404 mapping for KeyNotFoundException
app.MapGet("/not-found", () =>
{
    throw new KeyNotFoundException("Resource not found");
})
.WithTags("Errors");

// AggregateException (single inner → automatically unwrapped to the inner exception)
app.MapGet("/aggregate", () =>
{
    throw new AggregateException(
        new InvalidOperationException("Unwrapped inner exception"));
})
.WithTags("Errors");

// AggregateException (multiple inners → handled by fallback as AGGREGATE)
app.MapGet("/aggregate-multi", () =>
{
    throw new AggregateException(
        new InvalidOperationException("Error 1"),
        new ArgumentException("Error 2"));
})
.WithTags("Errors");

// Rate-limited endpoint — exceeding 5 requests/min returns structured 429 response
app.MapGet("/limited", () => Results.Ok(new { message = "You got through!" }))
    .RequireRateLimiting("api")
    .WithTags("Rate Limiting");

// Localized error — send Accept-Language header to get translated error messages
// Examples: Accept-Language: fr → French, Accept-Language: es → Spanish
// Error codes are used as resource keys in .resx files under Resources/
app.MapGet("/localized-error", () =>
{
    throw new InvalidOperationException("This is a demo error");
})
.WithTags("Localization");

// Validation endpoint — demonstrates OverrideModelStateValidation with Minimal APIs
// POST with invalid JSON to see structured fieldErrors
app.MapPost("/validate", (ContactRequest request) =>
    Results.Ok(new { message = "Contact created", name = request.Name }))
.WithTags("Validation");

// FluentValidation endpoint — demonstrates FluentValidation integration (v1.4.0)
// POST with invalid data to see fieldErrors with automatic error code mapping
app.MapPost("/validate-fluent", (OrderRequest request) =>
{
    var validator = new OrderRequestValidator();
    var result = validator.Validate(request);
    if (!result.IsValid)
        throw new FluentValidation.ValidationException(result.Errors);

    return Results.Ok(new { message = "Order placed", product = request.ProductName });
})
.WithTags("Validation");

// Health check — lists all features demonstrated by this sample
app.MapGet("/", () => Results.Ok(new
{
    service = "IntegrationSample",
    version = "1.4.0",
    features = new[]
    {
        "OpenTelemetry Tracing",
        "OpenAPI Schema Generation",
        "Rate Limiting",
        "AggregateException Handling",
        "Error Message Localization",
        "Validation (OverrideModelStateValidation)",
        "FluentValidation Integration",
        "KebabCase Error Code Strategy",
        "Custom FallbackMessage"
    }
}))
.WithTags("Info");

app.Run();

/// <summary>
/// Simple validation model for the /validate endpoint.
/// Demonstrates that OverrideModelStateValidation works with Minimal APIs.
/// </summary>
public class ContactRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
    public int Priority { get; set; }
}

/// <summary>
/// Request model for the /validate-fluent endpoint.
/// Validated via FluentValidation instead of DataAnnotations.
/// </summary>
public class OrderRequest
{
    public string? ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// FluentValidation validator for OrderRequest.
/// Error codes are automatically mapped to ErrorLens DefaultErrorCodes.
/// With KebabCase strategy, exception-based codes use kebab format.
/// </summary>
public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty();        // → REQUIRED_NOT_EMPTY
        RuleFor(x => x.Price).GreaterThan(0);          // → INVALID_MIN
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100); // → VALUE_OUT_OF_RANGE
        RuleFor(x => x.Email)
            .EmailAddress()                             // → INVALID_EMAIL
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
