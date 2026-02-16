using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using Microsoft.AspNetCore.Http;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles BadHttpRequestException from model binding failures.
/// </summary>
public class BadRequestExceptionHandler : AbstractApiExceptionHandler
{
    /// <inheritdoc />
    public override int Order => 150;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is BadHttpRequestException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        if (exception is not BadHttpRequestException badRequestException)
            throw new InvalidOperationException($"Cannot handle exception of type {exception.GetType()}. Call CanHandle first.");

        var statusCode = (HttpStatusCode)badRequestException.StatusCode;
        var message = SanitizeMessage(badRequestException.Message);

        var response = new ApiErrorResponse(
            statusCode,
            DefaultErrorCodes.BadRequest,
            message);

        return response;
    }

    private static string SanitizeMessage(string message)
    {
        // Replace Kestrel-internal messages that may contain type names,
        // framework-specific text, or internal details
        if (message.Contains("Microsoft.") ||
            message.Contains("System.") ||
            message.Contains("failed to read", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("unexpected end", StringComparison.OrdinalIgnoreCase))
        {
            return "Bad request";
        }

        return message;
    }
}
