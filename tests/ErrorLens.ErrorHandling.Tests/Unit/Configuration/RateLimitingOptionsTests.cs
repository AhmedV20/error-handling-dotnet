using ErrorLens.ErrorHandling.RateLimiting;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Configuration;

public class RateLimitingOptionsTests
{
    [Fact]
    public void ErrorCode_DefaultIsRateLimitExceeded()
    {
        var options = new RateLimitingOptions();

        options.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public void DefaultMessage_HasExpectedValue()
    {
        var options = new RateLimitingOptions();

        options.DefaultMessage.Should().Be("Too many requests. Please try again later.");
    }

    [Fact]
    public void IncludeRetryAfterInBody_DefaultIsTrue()
    {
        var options = new RateLimitingOptions();

        options.IncludeRetryAfterInBody.Should().BeTrue();
    }

    [Fact]
    public void UseModernHeaderFormat_DefaultIsFalse()
    {
        var options = new RateLimitingOptions();

        options.UseModernHeaderFormat.Should().BeFalse();
    }

    [Fact]
    public void ErrorCode_CanBeCustomized()
    {
        var options = new RateLimitingOptions
        {
            ErrorCode = "THROTTLED"
        };

        options.ErrorCode.Should().Be("THROTTLED");
    }

    [Fact]
    public void DefaultMessage_CanBeCustomized()
    {
        var options = new RateLimitingOptions
        {
            DefaultMessage = "Slow down!"
        };

        options.DefaultMessage.Should().Be("Slow down!");
    }

    [Fact]
    public void IncludeRetryAfterInBody_CanBeDisabled()
    {
        var options = new RateLimitingOptions
        {
            IncludeRetryAfterInBody = false
        };

        options.IncludeRetryAfterInBody.Should().BeFalse();
    }

    [Fact]
    public void UseModernHeaderFormat_CanBeEnabled()
    {
        var options = new RateLimitingOptions
        {
            UseModernHeaderFormat = true
        };

        options.UseModernHeaderFormat.Should().BeTrue();
    }
}
