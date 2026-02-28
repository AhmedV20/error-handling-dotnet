using System.Net;
using System.Text.Json;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles JSON parsing exceptions.
/// </summary>
public class JsonExceptionHandler : AbstractApiExceptionHandler
{
    private readonly ErrorHandlingOptions _options;

    public JsonExceptionHandler(IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

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
        var message = _options.BuiltInMessages.TryGetValue(DefaultErrorCodes.MessageNotReadable, out var custom)
            ? custom
            : "The request body could not be parsed as valid JSON";

        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.MessageNotReadable,
            message);
    }
}
