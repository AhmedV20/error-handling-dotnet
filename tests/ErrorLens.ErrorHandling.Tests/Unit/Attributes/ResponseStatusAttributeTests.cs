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

    // --- Range validation tests (T033) ---

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(599)]
    public void Constructor_WithValidIntStatusCode_DoesNotThrow(int statusCode)
    {
        var attr = new ResponseStatusAttribute(statusCode);

        attr.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(600)]
    [InlineData(99999)]
    public void Constructor_WithInvalidIntStatusCode_ThrowsArgumentOutOfRangeException(int statusCode)
    {
        var act = () => new ResponseStatusAttribute(statusCode);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("statusCode");
    }

    [ResponseStatus(HttpStatusCode.NotFound)]
    private class TestExceptionWithStatus : Exception { }
}
