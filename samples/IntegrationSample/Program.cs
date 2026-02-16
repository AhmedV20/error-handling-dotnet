using System.Threading.RateLimiting;
using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.OpenApi;
using ErrorLens.ErrorHandling.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- Error Handling ---
builder.Services.AddErrorHandling(options =>
{
    options.HttpStatusInJsonResponse = true;
    options.OverrideModelStateValidation = true;
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

// Middleware pipeline
app.UseErrorHandling();
app.UseRateLimiter();
app.MapOpenApi();

// --- Demo Endpoints ---

// Basic error
app.MapGet("/error", () =>
{
    throw new InvalidOperationException("This is a demo error");
})
.WithTags("Errors");

// Not found
app.MapGet("/not-found", () =>
{
    throw new KeyNotFoundException("Resource not found");
})
.WithTags("Errors");

// AggregateException (single inner → unwrapped)
app.MapGet("/aggregate", () =>
{
    throw new AggregateException(
        new InvalidOperationException("Unwrapped inner exception"));
})
.WithTags("Errors");

// AggregateException (multi → fallback)
app.MapGet("/aggregate-multi", () =>
{
    throw new AggregateException(
        new InvalidOperationException("Error 1"),
        new ArgumentException("Error 2"));
})
.WithTags("Errors");

// Rate-limited endpoint
app.MapGet("/limited", () => Results.Ok(new { message = "You got through!" }))
    .RequireRateLimiting("api")
    .WithTags("Rate Limiting");

// Health check
app.MapGet("/", () => Results.Ok(new
{
    service = "IntegrationSample",
    version = "1.3.0",
    features = new[]
    {
        "OpenTelemetry Tracing",
        "OpenAPI Schema Generation",
        "Rate Limiting",
        "AggregateException Handling"
    }
}))
.WithTags("Info");

app.Run();
