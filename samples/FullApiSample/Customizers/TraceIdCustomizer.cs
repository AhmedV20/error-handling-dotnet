using System.Diagnostics;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;

namespace FullApiSample.Customizers;

/// <summary>
/// Adds trace ID and timestamp to all error responses.
/// </summary>
public class TraceIdCustomizer : IApiErrorResponseCustomizer
{
    public void Customize(ApiErrorResponse response)
    {
        // Add trace ID for distributed tracing
        response.AddProperty("traceId", Activity.Current?.Id ?? Guid.NewGuid().ToString());

        // Add timestamp
        response.AddProperty("timestamp", DateTime.UtcNow.ToString("o"));

        // Add environment info
        response.AddProperty("instance", Environment.MachineName);
    }
}
