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
        var badRequestException = (BadHttpRequestException)exception;

        var response = new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            DefaultErrorCodes.BadRequest,
            badRequestException.Message);

        return response;
    }
}
