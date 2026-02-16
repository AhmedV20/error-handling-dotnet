using ErrorLens.ErrorHandling.Extensions;
using FullApiSample.Customizers;
using FullApiSample.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add error handling with configuration
builder.Services.AddErrorHandling(builder.Configuration);

// Register custom exception handler
builder.Services.AddApiExceptionHandler<BusinessExceptionHandler>();

// Register response customizer
builder.Services.AddErrorResponseCustomizer<TraceIdCustomizer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Use error handling middleware
app.UseErrorHandling();

app.MapControllers();

app.Run();
