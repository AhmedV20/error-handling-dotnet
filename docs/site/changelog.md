# Changelog

All notable changes to ErrorLens.ErrorHandling will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.3.1] - 2026-02-21

### Added

- `IncludeRejectedValues` option (default: `true`) — suppress rejected values in validation errors for sensitive data

### Fixed

- **Pipeline Resilience**: Localization and telemetry errors no longer crash the error handling pipeline (moved inside try/catch)
- **OperationCanceledException**: Client-aborted requests now propagate naturally instead of returning 500 JSON
- **Field-Specific Localization**: `LocalizeFieldError` tries composite key (`fieldName.errorCode`) before falling back to error code
- **ProblemDetails Custom Field Names**: Extension keys now use configured `JsonFieldNames` instead of hardcoded values
- **ProblemDetails Null Code**: `BuildTypeUri` returns `about:blank` for null error codes instead of throwing
- **AggregateExceptionHandler**: Cached handler resolution with type-based self-filtering (performance)
- **Rate Limiting Headers**: `RateLimit` header no longer includes invalid `limit=0`; `Retry-After` rounds up to minimum 1 second
- **BadRequestExceptionHandler**: Switched to allowlist-based message sanitization
- **Field Name Validation**: Case-insensitive duplicate detection for `JsonFieldNames`
- **ErrorCodeMapper**: Empty string guard returns `UNKNOWN_ERROR` instead of empty code
- **Null Guards**: Added `ArgumentNullException` guards for `message` parameter in `ApiFieldError`, `ApiGlobalError`, `ApiParameterError`
- **Serialization**: Cached `HashSet<string>` for built-in field names (avoids per-write allocation)

### Documentation

- Updated all configuration examples with `IncludeRejectedValues` option
- Updated sample YAML/JSON configs
- Improved documentation site content

## [1.3.0] - 2026-02-16

### Breaking Changes

- **`AddExceptionHandler<T>()` renamed to `AddApiExceptionHandler<T>()`**
  - Method renamed to avoid collision with ASP.NET Core 8+ built-in `AddExceptionHandler<T>()`
  - **Migration**: Update any custom handler registrations from `AddExceptionHandler<MyHandler>()` to `AddApiExceptionHandler<MyHandler>()`

### Added

- **OpenTelemetry Distributed Tracing**
  - `ErrorHandlingActivitySource` with shared `ActivitySource` ("ErrorLens.ErrorHandling")
  - Automatic `Activity` spans in `ErrorHandlingFacade.HandleException()` with OTel semantic conventions
  - Tags: `error.code`, `error.type`, `http.response.status_code`
  - Exception event with `exception.type`, `exception.message`, `exception.stacktrace`
  - Zero new dependencies — uses `System.Diagnostics.Activity` from runtime
  - Zero overhead when no `ActivityListener` is subscribed

- **Error Message Localization**
  - `IErrorMessageLocalizer` abstraction with `Localize()` and `LocalizeFieldError()` methods
  - `NoOpErrorMessageLocalizer` default (pass-through, registered via `TryAddSingleton`)
  - `StringLocalizerErrorMessageLocalizer<TResource>` bridge to `IStringLocalizer<T>`
  - Opt-in via `AddErrorHandlingLocalization<TResource>()`
  - Localizes top-level message, field errors, global errors, and parameter errors
  - New dependency: `Microsoft.Extensions.Localization.Abstractions 8.0.0`

- **New Package: ErrorLens.ErrorHandling.OpenApi** (.NET 9+)
  - `ErrorResponseOperationTransformer` implementing `IOpenApiOperationTransformer`
  - Auto-adds error response schemas (400, 404, 500) to all OpenAPI operations
  - Respects existing `[ProducesResponseType]` attributes
  - Reflects `UseProblemDetailFormat` and custom `JsonFieldNamesOptions` in generated schemas
  - Setup: `services.AddErrorHandlingOpenApi()`

- **New Package: ErrorLens.ErrorHandling.Swashbuckle** (.NET 6-8)
  - `ErrorResponseOperationFilter` implementing `IOperationFilter`
  - Same auto-schema behavior as OpenApi package for Swashbuckle/Swagger
  - Setup: `services.AddErrorHandlingSwashbuckle()`

- **Rate Limiting Integration** (.NET 7+)
  - `IRateLimitResponseWriter` interface for structured 429 responses
  - `DefaultRateLimitResponseWriter` with `Retry-After` and `RateLimit` headers
  - `RateLimitingOptions` for configuring error code, message, and header format
  - Optional `retryAfter` property in response body
  - Supports localized rate limit messages

- **`ErrorResponseWriter`**
  - Centralized response writing extracted into shared service
  - Caches `JsonSerializerOptions` for performance

- **`ErrorHandlingOptionsValidator`**
  - Validates `JsonFieldNamesOptions` at startup (null, empty, duplicate checks)

- **AggregateExceptionHandler** (Order 50)
  - Built-in handler for `AggregateException` types
  - Unwraps single-inner-exception aggregates and re-dispatches to appropriate specific handler
  - Multi-exception aggregates delegate to fallback handler
  - Automatically registered with default configuration

- **5xx Safe Message Behavior**
  - All 5xx-class errors (500-599) now return generic safe message: "An unexpected error occurred"
  - Prevents information disclosure of internal system details (database connection strings, file paths, stack traces)
  - 4xx errors preserve original user-facing messages

- **TypeMismatchExceptionHandler Generic Message**
  - Handler now returns generic "A type conversion error occurred" message
  - Prevents exposure of internal type conversion details

- **BadRequestStatusCode Usage**
  - Handler now uses actual `StatusCode` property from `BadHttpRequestException`
  - Correctly returns 400, 408, 413, etc. based on framework's status code

- **Message Sanitization**
  - `BadRequestExceptionHandler` sanitizes Kestrel-internal error messages
  - Replaces framework-specific details with safe "Bad request" message

- **OperationCanceledException → 499**
  - `OperationCanceledException` and `TaskCanceledException` now map to HTTP 499 (Client Closed Request)
  - Follows nginx convention for client-disconnected requests

- **reloadOnChange Default Changed**
  - YAML configuration `reloadOnChange` now defaults to `false`
  - Documented that application restart is required for config changes

- **ResponseErrorPropertyAttribute Cleanup**
  - Removed `AttributeTargets.Method` from `AttributeUsage`
  - Now targets `AttributeTargets.Property` only (as documented)

### Changed

- `AddApiExceptionHandler<T>()` now uses `TryAddEnumerable` for idempotent registration (prevents duplicates on double-call)
- `AddErrorResponseCustomizer<T>()` now uses `TryAddEnumerable` for idempotent registration
- Integration packages use `IOptions<T>` pattern instead of `BuildServiceProvider()` anti-pattern
- All package versions aligned to 1.3.0

### Removed

- `IHttpStatusFromExceptionMapper` interface (orphaned, never implemented or registered)
- `DefaultLoggingFilter` class (replaced by `ILoggingFilter` enumerable pattern)

### Fixed

- **Operator Precedence Bug**
  - `ModelStateValidationExceptionHandler.InferValidationType` fixed with explicit parentheses
  - Messages containing "json" no longer misclassified when combined with other keywords

- **Safe Pattern Matching**
  - All handler `Handle()` methods now use safe pattern matching (`is not`)
  - Prevents `InvalidCastException` when handlers called without prior `CanHandle()` check

- **Unified ToCamelCase**
  - `StringUtils.ToCamelCase` now shared across `ModelStateValidationExceptionHandler`, `ValidationExceptionHandler`, and `ExceptionMetadataCache`
  - Ensures consistent dotted-path handling (e.g., `Address.ZipCode` → `address.zipCode`)

- **ErrorCodeMapper Acronym Regex**
  - Fixed regex to correctly handle consecutive uppercase letters (acronyms)
  - `HTTPConnection` → `HTTP_CONNECTION` (not `H_T_T_P_CONNECTION`)
  - `IOError` → `IO_ERROR` (not `I_O_ERROR`)

- **Null Safety**
  - Added `ArgumentNullException.ThrowIfNull` guards to all model constructors
  - Added `ArgumentOutOfRangeException` for `ResponseStatusAttribute` (valid HTTP range: 100-599)

- **Serialization Fixes**
  - `ApiErrorResponseConverter` filters duplicate JSON keys (extension properties matching built-in field names)
  - `RejectedValue` serialization now passes parent `JsonSerializerOptions` for consistent naming policy
  - `ApiErrorResponseConverter.Read` throws `NotSupportedException` (write-only contract)

- **ProblemDetailFactory Fixes**
  - `ProblemDetailConvertToKebabCase` option now respected (false skips kebab-case conversion)
  - Extension keys can no longer overwrite library-set keys (`type`, `title`, `status`, etc.)
  - List contents copied instead of sharing mutable references

- **Pipeline Resilience**
  - `ErrorResponseWriter` checks `Response.HasStarted` before writing (graceful skip if response already started)
  - `ErrorHandlingMiddleware` passes `context.RequestAborted` as `CancellationToken` to response writer
  - `ErrorHandlingFacade` uses `ExceptionDispatchInfo.Capture(exception).Throw()` to preserve stack trace when disabled
  - `ErrorHandlingFacade` materializes `_customizers` with `.ToList()` to prevent lazy enumerable issues
  - Safety-net in facade logs both handler exception AND original exception when handler throws

- **DI Improvements**
  - Built-in handler registrations use `TryAddEnumerable` for idempotency
  - Multiple `AddErrorHandling()` calls no longer create duplicate handler registrations

### Test Coverage

- **New Tests**: 165 new tests added (total: 350 tests, up from 185)
  - Null safety tests for all model constructors
  - Range validation tests for `ResponseStatusAttribute`
  - Startup validation tests for `JsonFieldNamesOptions`
  - Handler tests: `AggregateExceptionHandler`, `TypeMismatchExceptionHandler`, `BadRequestExceptionHandler`
  - Mapper tests: acronym regex, `OperationCanceledException` → 499
  - Integration tests: middleware, exception handler, DI registration
  - Security tests: 10+ information disclosure scenarios
  - Serialization tests: duplicate key filtering, `RejectedValue` options, `Read` throws

- **Fixed Tests**
  - `ErrorResponseWriterTests`: Replaced false-positive caching and cancellation tests with real assertions
  - `ProblemDetailFactoryTests`: Fixed swapped `ApiFieldError` constructor args
  - `LoggingServiceTests`: Replaced weak `.ReceivedCalls()` assertions with specific log level verification
  - `ErrorHandlingFacadeTests`: Added safety-net test (handler throws → both exceptions logged)

- **Coverage**: 93.1% line coverage (exceeds 90% requirement)

### CI/CD

- CI and Release workflows updated to pack and publish all 3 NuGet packages
- Release summary includes links for all 3 packages

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
