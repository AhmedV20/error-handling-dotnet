using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ErrorLens.ErrorHandling.Attributes;

namespace ErrorLens.ErrorHandling.Internal;

/// <summary>
/// Caches exception metadata from attributes for performance.
/// </summary>
internal static class ExceptionMetadataCache
{
    private static readonly ConcurrentDictionary<Type, ExceptionMetadata> Cache = new();

    /// <summary>
    /// Gets cached metadata for an exception type.
    /// </summary>
    public static ExceptionMetadata GetMetadata(Type exceptionType)
    {
        return Cache.GetOrAdd(exceptionType, BuildMetadata);
    }

    private static ExceptionMetadata BuildMetadata(Type exceptionType)
    {
        var errorCodeAttr = exceptionType.GetCustomAttribute<ResponseErrorCodeAttribute>();
        var statusAttr = exceptionType.GetCustomAttribute<ResponseStatusAttribute>();

        var properties = exceptionType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ResponseErrorPropertyAttribute>() != null)
            .Select(p => new PropertyMetadata(
                p,
                p.GetCustomAttribute<ResponseErrorPropertyAttribute>()!))
            .ToList();

        return new ExceptionMetadata(
            errorCodeAttr?.Code,
            statusAttr?.StatusCode,
            properties);
    }
}

/// <summary>
/// Cached metadata for an exception type.
/// </summary>
internal sealed class ExceptionMetadata
{
    public string? ErrorCode { get; }
    public HttpStatusCode? StatusCode { get; }
    public IReadOnlyList<PropertyMetadata> Properties { get; }

    public ExceptionMetadata(
        string? errorCode,
        HttpStatusCode? statusCode,
        IReadOnlyList<PropertyMetadata> properties)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Properties = properties;
    }
}

/// <summary>
/// Metadata for a property marked with ResponseErrorPropertyAttribute.
/// </summary>
internal sealed class PropertyMetadata
{
    public PropertyInfo Property { get; }
    public string Name { get; }
    public bool IncludeIfNull { get; }

    public PropertyMetadata(PropertyInfo property, ResponseErrorPropertyAttribute attribute)
    {
        Property = property;
        Name = attribute.Name ?? ToCamelCase(property.Name);
        IncludeIfNull = attribute.IncludeIfNull;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
