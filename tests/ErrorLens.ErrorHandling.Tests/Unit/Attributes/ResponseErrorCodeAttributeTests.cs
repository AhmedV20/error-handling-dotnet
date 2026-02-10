using System.Net;
using ErrorLens.ErrorHandling.Attributes;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Attributes;

public class ResponseErrorCodeAttributeTests
{
    [Fact]
    public void Constructor_SetsCode()
    {
        var attr = new ResponseErrorCodeAttribute("USER_NOT_FOUND");

        attr.Code.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        var act = () => new ResponseErrorCodeAttribute(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        var attr = typeof(TestExceptionWithCode)
            .GetCustomAttributes(typeof(ResponseErrorCodeAttribute), false)
            .FirstOrDefault() as ResponseErrorCodeAttribute;

        attr.Should().NotBeNull();
        attr!.Code.Should().Be("TEST_ERROR");
    }

    [ResponseErrorCode("TEST_ERROR")]
    private class TestExceptionWithCode : Exception { }
}
