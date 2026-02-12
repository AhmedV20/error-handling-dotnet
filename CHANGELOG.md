# Changelog

All notable changes to ErrorLens.ErrorHandling will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned

- OpenTelemetry integration for distributed tracing
- Swagger/OpenAPI schema generation for error responses
- Localization support for error messages
- Rate limiting integration

## [1.1.1] - 2026-02-12

### Fixed

- **Critical Bug**: `ILoggingService` was not registered in DI container, causing exception logging to be silently disabled in v1.1.0
  - Added `services.TryAddSingleton<ILoggingService, LoggingService>()` to `RegisterCoreServices()`
  - Logging now works correctly with all providers (console, Seq, Serilog, etc.)
- **Documentation**: Handler ordering table in `custom-handlers.md` and `index.adoc` now clearly marks `JsonExceptionHandler` and `TypeMismatchExceptionHandler` as opt-in (not registered by default)

### Added

- **Sample Project READMEs**: Comprehensive documentation for all three samples
  - MinimalApiSample: Zero-config setup guide with curl examples
  - FullApiSample: Custom handlers and customizers walkthrough
  - ShowcaseSample: Feature matrix with YAML config explanation

## [1.1.0] - 2026-02-11

### Added

- **Custom JSON Field Names**
  - `JsonFieldNamesOptions` now fully wired into serialization pipeline
  - Custom `ApiErrorResponseConverter` for runtime-configurable field names
  - Rename any JSON property: `code` → `type`, `message` → `detail`, etc.
  - Applies to top-level response and all nested error objects
  - Expanded from 5 to 10 configurable field names (added Status, Property, RejectedValue, Path, Parameter)

- **YAML Configuration Support**
  - `AddYamlErrorHandling()` extension method for `IConfigurationBuilder`
  - Full `errorhandling.yml` template with all options documented
  - Same `ErrorHandling` section name as JSON configuration
  - Dependency: NetEscapades.Configuration.Yaml 3.1.0

- **ShowcaseSample Project**
  - Comprehensive sample demonstrating all features in one project
  - 5 controllers: BasicErrors, Validation, Attributes, ConfigDriven, ProblemDetails
  - Custom exception handler (InfrastructureExceptionHandler)
  - Response customizer (RequestMetadataCustomizer) with traceId, timestamp, path
  - YAML configuration with custom field names, exception mappings, log levels

- **Professional Documentation**
  - AsciiDoc index page (`docs/index.adoc`) with feature matrix
  - Guides: Getting Started, Configuration, Validation Errors, Logging
  - Features: Custom Handlers, Attributes, Response Customization, Problem Details, JSON Field Names
  - API Reference with all public types, interfaces, and extension methods
  - Full YAML configuration template

- **OverrideModelStateValidation Option**
  - Opt-in interception of `[ApiController]` automatic model validation
  - Returns ErrorLens structured `fieldErrors` format instead of ASP.NET Core's default `ProblemDetails`
  - `ModelStateValidationExceptionHandler` (Order 90) converts `ModelStateDictionary` into structured response
  - Configurable via code, JSON, or YAML (`OverrideModelStateValidation: true`)

- **Expanded Test Coverage**
  - 18 unit tests for ApiErrorResponseConverter
  - 5 integration tests for JsonFieldNames end-to-end
  - 10 integration tests for YAML configuration binding
  - Total: 175 tests (up from 142)

### Changed

- README.md rewritten with badges, YAML/JSON side-by-side config, custom field names section, samples table

## [1.0.0] - 2026-02-10

### Added

- **Zero-Config Exception Handling**
  - Automatic structured JSON error responses for all unhandled exceptions
  - Default error code generation using ALL_CAPS strategy
  - HTTP status code mapping based on exception types

- **Validation Error Support**
  - Field-level validation error details with `fieldErrors` array
  - Property name, error code, message, and rejected value included
  - Support for DataAnnotations validation exceptions
  - Global and parameter-level error support

- **Configuration via appsettings.json**
  - Custom error codes per exception type
  - Custom error messages per exception type
  - HTTP status code overrides
  - Field-specific error code/message customization
  - Superclass hierarchy search option

- **Custom Exception Attributes**
  - `[ResponseErrorCode]` - Define custom error code on exception class
  - `[ResponseErrorProperty]` - Include exception property in response
  - `[ResponseStatus]` - Define HTTP status code on exception class
  - Reflection caching for performance

- **Custom Exception Handlers**
  - `IApiExceptionHandler` interface for custom handling
  - `AbstractApiExceptionHandler` base class with default Order
  - Priority ordering (lower Order = higher priority)
  - Built-in handlers: ModelStateValidation, Validation, BadRequest (Json and TypeMismatch available but not registered by default)

- **RFC 9457 Problem Details Format**
  - Optional RFC 9457 compliant response format
  - Configurable type URI prefix
  - Automatic kebab-case conversion for error codes
  - Extension properties for additional data

- **Logging Configuration**
  - Configurable logging verbosity (None, MessageOnly, WithStacktrace)
  - Per-HTTP-status log level configuration
  - Full stacktrace options per status code or exception type
  - Custom logging filters

- **Response Customization**
  - `IApiErrorResponseCustomizer` interface
  - Add global properties to all error responses
  - Multiple customizers with order of execution

- **Multi-Target Framework Support**
  - .NET 6.0, 7.0, 8.0, 9.0, 10.0
  - `IExceptionHandler` for .NET 8+ (native)
  - `IMiddleware` for .NET 6/7 (fallback)

### Technical Details

- System.Text.Json for serialization
- Thread-safe reflection caching with ConcurrentDictionary
- Minimal dependencies (Microsoft.Extensions.Options, Microsoft.Extensions.Logging.Abstractions)
- Full XML documentation
- 142 unit and integration tests
