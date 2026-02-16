using System.Diagnostics;
using ErrorLens.ErrorHandling.Telemetry;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Telemetry;

[Collection("Telemetry")]
public class ErrorHandlingActivitySourceTests
{
    [Fact]
    public void ActivitySourceName_IsErrorLensErrorHandling()
    {
        ErrorHandlingActivitySource.ActivitySourceName.Should().Be("ErrorLens.ErrorHandling");
    }

    [Fact]
    public void Source_IsNotNull()
    {
        ErrorHandlingActivitySource.Source.Should().NotBeNull();
    }

    [Fact]
    public void Source_IsActivitySource()
    {
        ErrorHandlingActivitySource.Source.Should().BeOfType<ActivitySource>();
    }

    [Fact]
    public void Source_HasCorrectName()
    {
        ErrorHandlingActivitySource.Source.Name.Should().Be("ErrorLens.ErrorHandling");
    }
}
