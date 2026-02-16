using ErrorLens.ErrorHandling.Internal;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Internal;

public class StringUtilsTests
{
    [Fact]
    public void ToCamelCase_PascalCase_ReturnsCamelCase()
    {
        StringUtils.ToCamelCase("UserName").Should().Be("userName");
    }

    [Fact]
    public void ToCamelCase_DottedPath_ConvertsEachSegment()
    {
        StringUtils.ToCamelCase("Address.ZipCode").Should().Be("address.zipCode");
    }

    [Fact]
    public void ToCamelCase_ThreeSegments_ConvertsAll()
    {
        StringUtils.ToCamelCase("User.Address.ZipCode").Should().Be("user.address.zipCode");
    }

    [Fact]
    public void ToCamelCase_SingleChar_ReturnsLowerCase()
    {
        StringUtils.ToCamelCase("X").Should().Be("x");
    }

    [Fact]
    public void ToCamelCase_EmptyString_ReturnsEmpty()
    {
        StringUtils.ToCamelCase("").Should().Be("");
    }

    [Fact]
    public void ToCamelCase_Null_ReturnsNull()
    {
        StringUtils.ToCamelCase(null!).Should().BeNull();
    }

    [Fact]
    public void ToCamelCase_AlreadyCamelCase_ReturnsSame()
    {
        StringUtils.ToCamelCase("userName").Should().Be("userName");
    }

    [Fact]
    public void ToCamelCase_AllLowerCase_ReturnsSame()
    {
        StringUtils.ToCamelCase("email").Should().Be("email");
    }

    [Fact]
    public void ToCamelCase_DottedAlreadyCamel_ReturnsSame()
    {
        StringUtils.ToCamelCase("user.email").Should().Be("user.email");
    }
}
