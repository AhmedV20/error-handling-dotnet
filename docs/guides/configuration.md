# Configuration

ErrorLens.ErrorHandling supports both JSON (`appsettings.json`) and YAML (`errorhandling.yml`) configuration using the `ErrorHandling` section name.

## JSON Configuration

Configuration is automatically read from `appsettings.json`:

```json
{
  "ErrorHandling": {
    "Enabled": true,
    "HttpStatusInJsonResponse": true,
    "DefaultErrorCodeStrategy": "AllCaps",
    "SearchSuperClassHierarchy": true,
    "ExceptionLogging": "WithStacktrace"
  }
}
```

## YAML Configuration

Add YAML support with a single line:

```csharp
builder.Configuration.AddYamlErrorHandling("errorhandling.yml");
builder.Services.AddErrorHandling(builder.Configuration);
```

```yaml
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  SearchSuperClassHierarchy: true
  ExceptionLogging: WithStacktrace
```

A full YAML template is available at [`errorhandling-template.yml`](../errorhandling-template.yml).

## All Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable error handling globally |
| `HttpStatusInJsonResponse` | `bool` | `false` | Include HTTP status code in JSON body |
| `DefaultErrorCodeStrategy` | `enum` | `AllCaps` | `AllCaps` or `FullQualifiedName` |
| `SearchSuperClassHierarchy` | `bool` | `false` | Search base classes for config matches |
| `AddPathToError` | `bool` | `true` | Include property path in field errors |
| `UseProblemDetailFormat` | `bool` | `false` | Enable RFC 9457 Problem Details format |
| `ProblemDetailTypePrefix` | `string` | `https://example.com/errors/` | Type URI prefix for Problem Details |
| `ProblemDetailConvertToKebabCase` | `bool` | `true` | Convert error codes to kebab-case in type URI |
| `ExceptionLogging` | `enum` | `MessageOnly` | `NoLogging`, `MessageOnly`, `WithStacktrace` |

## Dictionary Mappings

### HttpStatuses

Map exception types to HTTP status codes:

```yaml
HttpStatuses:
  MyApp.Exceptions.UserNotFoundException: 404
  MyApp.Exceptions.DuplicateEmailException: 409
  MyApp.Exceptions.ForbiddenException: 403
```

### Codes

Map exception types or field validations to error codes:

```yaml
Codes:
  MyApp.Exceptions.UserNotFoundException: USER_NOT_FOUND
  email.Required: EMAIL_IS_REQUIRED
  email.EmailAddress: EMAIL_FORMAT_INVALID
```

### Messages

Map exception types or field validations to custom messages:

```yaml
Messages:
  MyApp.Exceptions.UserNotFoundException: The requested user was not found
  email.Required: A valid email address is required
```

### LogLevels

Map HTTP status codes to log levels:

```yaml
LogLevels:
  4xx: Warning
  5xx: Error
  404: Debug     # Override specific status within a range
```

### FullStacktraceHttpStatuses

Force full stack trace logging for specific HTTP status patterns:

```yaml
FullStacktraceHttpStatuses:
  - 5xx
  - 400
```

### FullStacktraceClasses

Force full stack trace logging for specific exception types:

```yaml
FullStacktraceClasses:
  - System.NullReferenceException
  - MyApp.Exceptions.CriticalException
```

## Configuration Priority

Configuration is resolved in this order (highest priority first):

1. Inline options (`Action<ErrorHandlingOptions>`)
2. Configuration binding (`IConfiguration`)
3. Exception attributes (`[ResponseErrorCode]`, `[ResponseStatus]`)
4. Default conventions (class name → ALL_CAPS)
