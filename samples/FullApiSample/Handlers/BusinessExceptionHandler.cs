using System.Net;
using ErrorLens.ErrorHandling.Handlers;
using ErrorLens.ErrorHandling.Models;
using FullApiSample.Exceptions;

namespace FullApiSample.Handlers;

/// <summary>
/// Custom handler for all business exceptions.
/// </summary>
public class BusinessExceptionHandler : AbstractApiExceptionHandler
{
    /// <summary>
    /// Higher priority than default handlers.
    /// </summary>
    public override int Order => 50;

    public override bool CanHandle(Exception exception)
        => exception is BusinessException;

    public override ApiErrorResponse Handle(Exception exception)
    {
        var businessEx = (BusinessException)exception;

        return new ApiErrorResponse(
            HttpStatusCode.BadRequest,
            "BUSINESS_RULE_VIOLATION",
            businessEx.Message);
    }
}
