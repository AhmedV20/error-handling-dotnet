using ErrorLens.ErrorHandling.Extensions;
using ShowcaseSample.Customizers;
using ShowcaseSample.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add YAML configuration for error handling
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");

// Add controllers
builder.Services.AddControllers();

// Add error handling (reads from IConfiguration — both JSON and YAML)
builder.Services.AddErrorHandling(builder.Configuration);

// Register custom infrastructure exception handler
builder.Services.AddExceptionHandler<InfrastructureExceptionHandler>();

// Register response customizer (adds traceId, timestamp, path)
builder.Services.AddHttpContextAccessor();
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Use error handling middleware
app.UseErrorHandling();

app.MapControllers();

app.Run();
