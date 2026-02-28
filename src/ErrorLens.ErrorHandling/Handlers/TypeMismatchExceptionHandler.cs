using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.Options;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles type mismatch exceptions (FormatException, InvalidCastException).
/// </summary>
public class TypeMismatchExceptionHandler : AbstractApiExceptionHandler
{
    private readonly ErrorHandlingOptions _options;

    public TypeMismatchExceptionHandler(IOptions<ErrorHandlingOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public override int Order => 130;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is FormatException or InvalidCastException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        if (exception is not (FormatException or InvalidCastException))
            throw new InvalidOperationException($"Cannot handle exception of type {exception.GetType()}. Call CanHandle first.");

        var message = _options.BuiltInMessages.TryGetValue(DefaultErrorCodes.TypeMismatch, out var custom)
            ? custom
            : "A type conversion error occurred";

        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.TypeMismatch,
            message);
    }
}
