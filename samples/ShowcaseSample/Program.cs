using ErrorLens.ErrorHandling.Extensions;
using ErrorLens.ErrorHandling.FluentValidation;
using ErrorLens.ErrorHandling.Swashbuckle;
using ShowcaseSample.Customizers;
using ShowcaseSample.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add YAML configuration for error handling
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");

// Add controllers
builder.Services.AddControllers();

// Add error handling (reads from IConfiguration — both JSON and YAML)
builder.Services.AddErrorHandling(builder.Configuration);

// Add FluentValidation error handling integration
builder.Services.AddErrorHandlingFluentValidation();

// Register custom infrastructure exception handler
builder.Services.AddApiExceptionHandler<InfrastructureExceptionHandler>();

// Register response customizer (adds traceId, timestamp, path)
builder.Services.AddHttpContextAccessor();
builder.Services.AddErrorResponseCustomizer<RequestMetadataCustomizer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ErrorLens error schemas to Swagger documentation
builder.Services.AddErrorHandlingSwashbuckle();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Use error handling middleware
app.UseErrorHandling();

app.MapControllers();

app.Run();
