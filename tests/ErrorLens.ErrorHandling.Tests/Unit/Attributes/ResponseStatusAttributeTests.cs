using System.Net;
using ErrorLens.ErrorHandling.Attributes;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Attributes;

public class ResponseStatusAttributeTests
{
    [Fact]
    public void Constructor_WithHttpStatusCode_SetsStatusCode()
    {
        var attr = new ResponseStatusAttribute(HttpStatusCode.NotFound);

        attr.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Constructor_WithInt_SetsStatusCode()
    {
        var attr = new ResponseStatusAttribute(404);

        attr.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        var attr = typeof(TestExceptionWithStatus)
            .GetCustomAttributes(typeof(ResponseStatusAttribute), false)
            .FirstOrDefault() as ResponseStatusAttribute;

        attr.Should().NotBeNull();
        attr!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ResponseStatus(HttpStatusCode.NotFound)]
    private class TestExceptionWithStatus : Exception { }
}
