# Configuration Template

Full YAML configuration template with all available options and their defaults.

```yaml
# ErrorLens.ErrorHandling Configuration Template
# Copy this file to your project as 'errorhandling.yml'
# and configure as needed.

ErrorHandling:
  # Enable or disable error handling globally
  Enabled: true

  # Include HTTP status code in JSON response body
  HttpStatusInJsonResponse: false

  # Error code generation strategy: AllCaps or FullQualifiedName
  DefaultErrorCodeStrategy: AllCaps

  # Search exception base class hierarchy for configuration matches
  SearchSuperClassHierarchy: false

  # Include property path in field error responses
  AddPathToError: true

  # Intercept [ApiController] model validation to use ErrorLens format
  OverrideModelStateValidation: false

  # Use RFC 9457 Problem Details format
  UseProblemDetailFormat: false

  # URI prefix for Problem Details 'type' field
  ProblemDetailTypePrefix: "https://example.com/errors/"

  # Convert error codes to kebab-case in Problem Details 'type' field
  ProblemDetailConvertToKebabCase: true

  # Exception logging verbosity: None, MessageOnly, WithStacktrace
  ExceptionLogging: MessageOnly

  # Custom JSON field names for error responses
  JsonFieldNames:
    Code: code
    Message: message
    Status: status
    FieldErrors: fieldErrors
    GlobalErrors: globalErrors
    ParameterErrors: parameterErrors
    Property: property
    RejectedValue: rejectedValue
    Path: path
    Parameter: parameter

  # Exception type → HTTP status code mappings
  HttpStatuses:
    # MyApp.Exceptions.UserNotFoundException: 404
    # MyApp.Exceptions.DuplicateEmailException: 409
    # MyApp.Exceptions.ForbiddenException: 403

  # Exception type or field.validation → error code mappings
  Codes:
    # MyApp.Exceptions.UserNotFoundException: USER_NOT_FOUND
    # email.Required: EMAIL_IS_REQUIRED
    # email.EmailAddress: EMAIL_FORMAT_INVALID

  # Exception type or field.validation → error message mappings
  Messages:
    # MyApp.Exceptions.UserNotFoundException: The requested user was not found
    # email.Required: A valid email address is required

  # HTTP status code or pattern → log level mappings
  LogLevels:
    # 4xx: Warning
    # 5xx: Error
    # 404: Debug

  # HTTP status codes/patterns that force full stack trace logging
  FullStacktraceHttpStatuses:
    # - 5xx
    # - 400

  # Exception types that force full stack trace logging
  FullStacktraceClasses:
    # - System.NullReferenceException
    # - MyApp.Exceptions.CriticalException

  # OpenAPI schema generation settings
  OpenApi:
    DefaultStatusCodes:
      - 400
      - 404
      - 500

  # Rate limiting response settings (.NET 7+)
  RateLimiting:
    ErrorCode: RATE_LIMIT_EXCEEDED
    DefaultMessage: "Too many requests. Please try again later."
    IncludeRetryAfterInBody: true
    UseModernHeaderFormat: false
```
