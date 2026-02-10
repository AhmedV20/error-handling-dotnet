using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles type mismatch exceptions (FormatException, InvalidCastException).
/// </summary>
public class TypeMismatchExceptionHandler : AbstractApiExceptionHandler
{
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
        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.TypeMismatch,
            exception.Message);
    }
}
