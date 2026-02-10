using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles JSON parsing exceptions.
/// </summary>
public class JsonExceptionHandler : AbstractApiExceptionHandler
{
    /// <inheritdoc />
    public override int Order => 120;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is JsonException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.MessageNotReadable,
            "The request body could not be parsed as valid JSON");
    }
}
