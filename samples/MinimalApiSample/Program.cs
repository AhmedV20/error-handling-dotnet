using ErrorLens.ErrorHandling.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add error handling - that's all you need!
builder.Services.AddErrorHandling();

var app = builder.Build();

// Use error handling middleware
app.UseErrorHandling();

// Demo endpoints that throw various exceptions
app.MapGet("/", () => "ErrorLens.ErrorHandling - Minimal API Sample");

app.MapGet("/error/generic", () =>
{
    throw new Exception("Something went wrong!");
});

app.MapGet("/error/invalid-operation", () =>
{
    throw new InvalidOperationException("This operation is not allowed");
});

app.MapGet("/error/argument", (string? name) =>
{
    if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(nameof(name), "Name is required");
    return $"Hello, {name}!";
});

app.MapGet("/error/not-found", (int id) =>
{
    throw new KeyNotFoundException($"Item with ID {id} not found");
});

app.MapGet("/error/unauthorized", () =>
{
    throw new UnauthorizedAccessException("You don't have permission to access this resource");
});

app.MapGet("/error/format", () =>
{
    throw new FormatException("The input string was not in a correct format");
});

app.MapGet("/error/timeout", () =>
{
    throw new TimeoutException("The operation has timed out");
});

app.MapGet("/error/not-implemented", () =>
{
    throw new NotImplementedException("This feature is coming soon");
});

app.Run();
