using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Services;
using FullApiSample.Customizers;
using FullApiSample.Filters;
using FullApiSample.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add error handling with configuration
builder.Services.AddErrorHandling(builder.Configuration);

// Register custom exception handler (Order 50 — runs before built-in handlers)
builder.Services.AddApiExceptionHandler<BusinessExceptionHandler>();


// Register response customizer (adds traceId, timestamp, instance to all errors)
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();

// Register logging filter (suppresses 404 logs to reduce noise)
builder.Services.AddSingleton<ILoggingFilter, IgnoreNotFoundFilter>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Use error handling middleware
app.UseErrorHandling();

app.MapControllers();

app.Run();
