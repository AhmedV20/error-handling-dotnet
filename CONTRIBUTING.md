# Contributing to ErrorLens.ErrorHandling

Thank you for your interest in contributing to ErrorLens.ErrorHandling! This document provides guidelines and instructions for contributing.

## Code of Conduct

Please be respectful and constructive in all interactions. We welcome contributors of all experience levels.

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in [Issues](https://github.com/AhmedV20/error-handling-dotnet/issues)
2. If not, create a new issue using the Bug Report template
3. Include a minimal reproducible example

### Suggesting Features

1. Check if the feature has already been requested
2. Create a new issue using the Feature Request template
3. Explain the use case and expected behavior

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Write or update tests
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## Development Setup

### Prerequisites

- .NET SDK 10.0 or later
- Git

### Building

```bash
git clone https://github.com/AhmedV20/error-handling-dotnet.git
cd error-handling-dotnet
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running Samples

```bash
# Minimal API Sample
cd samples/MinimalApiSample
dotnet run

# Full API Sample
cd samples/FullApiSample
dotnet run

# Showcase Sample
cd samples/ShowcaseSample
dotnet run

# Integration Sample (requires .NET 9.0+)
cd samples/IntegrationSample
dotnet run
```

## Coding Guidelines

### Style

- Follow existing code style
- Use meaningful names for variables, methods, and classes
- Keep methods focused and small
- Add XML documentation for public APIs

### Testing

- Write tests for all new functionality
- Follow TDD when possible
- Use FluentAssertions for assertions
- Use NSubstitute for mocking

### Commits

- Use clear, descriptive commit messages
- Reference issue numbers when applicable
- Keep commits focused on single changes

## Project Structure

```
src/
  ErrorLens.ErrorHandling/           # Core library (net6.0-net10.0)
    Attributes/                      # Custom attributes
    Configuration/                   # Options, validators, enums
    Extensions/                      # DI + middleware registration
    Handlers/                        # Exception handlers
    Integration/                     # Middleware and IExceptionHandler
    Internal/                        # ExceptionMetadataCache, StringUtils
    Localization/                    # IErrorMessageLocalizer + implementations
    Mappers/                         # Error code/message/status mappers
    Models/                          # Response models
    OpenApi/                         # Schema generator (shared)
    ProblemDetails/                   # RFC 9457 support
    RateLimiting/                    # Rate limit response writer (.NET 7+)
    Serialization/                   # Custom JSON converter
    Services/                        # ErrorHandlingFacade, LoggingService
    Telemetry/                       # OpenTelemetry ActivitySource

  ErrorLens.ErrorHandling.OpenApi/   # OpenAPI transformer (.NET 9+)
  ErrorLens.ErrorHandling.Swashbuckle/ # Swashbuckle filter (.NET 6-8)

tests/
  ErrorLens.ErrorHandling.Tests/     # Core library tests
    Contract/                        # Contract tests
    Integration/                     # Integration tests
    Unit/                            # Unit tests
  ErrorLens.ErrorHandling.OpenApi.Tests/      # OpenAPI tests
  ErrorLens.ErrorHandling.Swashbuckle.Tests/  # Swashbuckle tests

samples/
  MinimalApiSample/                  # Zero-config Minimal API example
  FullApiSample/                     # Full-featured controller API example
  ShowcaseSample/                    # All features showcase with Swashbuckle
  IntegrationSample/                 # OpenTelemetry, OpenAPI, Rate Limiting, Localization
```

## Questions?

Feel free to open an issue for any questions or concerns.

Thank you for contributing!
