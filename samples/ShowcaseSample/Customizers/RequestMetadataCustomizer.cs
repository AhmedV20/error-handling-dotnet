using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;

namespace ShowcaseSample.Customizers;

/// <summary>
/// Adds request metadata (traceId, timestamp, path) to all error responses.
/// Demonstrates implementing IApiErrorResponseCustomizer.
/// </summary>
public class RequestMetadataCustomizer : IApiErrorResponseCustomizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestMetadataCustomizer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Customize(ApiErrorResponse response)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        response.AddProperty("traceId", context.TraceIdentifier);
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));
        response.AddProperty("path", context.Request.Path.Value);
    }
}
