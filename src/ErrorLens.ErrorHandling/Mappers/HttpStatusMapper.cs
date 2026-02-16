using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Mappers;

/// <summary>
/// Maps exceptions to HTTP status codes based on configuration and conventions.
/// </summary>
public class HttpStatusMapper : IHttpStatusMapper
{
    private readonly ErrorHandlingOptions _options;

    public HttpStatusMapper(
        IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public HttpStatusCode GetHttpStatus(Exception exception)
    {
        return GetHttpStatus(exception, HttpStatusCode.InternalServerError);
    }

    /// <inheritdoc />
    public HttpStatusCode GetHttpStatus(Exception exception, HttpStatusCode defaultStatus)
    {
        var exceptionType = exception.GetType();
        var typeName = exceptionType.FullName ?? exceptionType.Name;

        // Check configuration for explicit mapping
        if (_options.HttpStatuses.TryGetValue(typeName, out var configuredStatus))
        {
            return configuredStatus;
        }

        // Check superclass hierarchy if enabled
        if (_options.SearchSuperClassHierarchy)
        {
            var baseType = exceptionType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var baseTypeName = baseType.FullName ?? baseType.Name;
                if (_options.HttpStatuses.TryGetValue(baseTypeName, out var baseStatus))
                {
                    return baseStatus;
                }
                baseType = baseType.BaseType;
            }
        }

        // Apply default mappings for common exception types
        // Note: Order matters - more specific types must come before base types
        return exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            NotImplementedException => HttpStatusCode.NotImplemented,
            TimeoutException => HttpStatusCode.RequestTimeout,
            OperationCanceledException => (HttpStatusCode)499,
            KeyNotFoundException => HttpStatusCode.NotFound,
            FileNotFoundException => HttpStatusCode.NotFound,
            DirectoryNotFoundException => HttpStatusCode.NotFound,
            FormatException => HttpStatusCode.BadRequest,
            _ => defaultStatus
        };
    }
}
