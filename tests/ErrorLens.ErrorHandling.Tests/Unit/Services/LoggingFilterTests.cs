using System.Net;
using ErrorLens.ErrorHandling.Models;
using ErrorLens.ErrorHandling.Services;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Services;

public class LoggingFilterTests
{
    [Fact]
    public void DefaultLoggingFilter_AlwaysReturnsTrue()
    {
        var filter = new DefaultLoggingFilter();
        var response = new ApiErrorResponse(HttpStatusCode.BadRequest, "ERROR", "Test");
        var exception = new Exception("test");

        var result = filter.ShouldLog(response, exception);

        result.Should().BeTrue();
    }

    [Fact]
    public void CustomLoggingFilter_CanExcludeExceptions()
    {
        var filter = new Custom404Filter();
        var response404 = new ApiErrorResponse(HttpStatusCode.NotFound, "NOT_FOUND", "Not found");
        var response500 = new ApiErrorResponse(HttpStatusCode.InternalServerError, "ERROR", "Error");

        filter.ShouldLog(response404, new Exception()).Should().BeFalse();
        filter.ShouldLog(response500, new Exception()).Should().BeTrue();
    }

    private class Custom404Filter : ILoggingFilter
    {
        public bool ShouldLog(ApiErrorResponse response, Exception exception)
        {
            return response.HttpStatusCode != HttpStatusCode.NotFound;
        }
    }
}
