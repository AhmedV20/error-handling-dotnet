using ErrorLens.ErrorHandling.Attributes;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Attributes;

public class ResponseErrorPropertyAttributeTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var attr = new ResponseErrorPropertyAttribute();

        attr.Name.Should().BeNull();
        attr.IncludeIfNull.Should().BeFalse();
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var attr = new ResponseErrorPropertyAttribute { Name = "customName" };

        attr.Name.Should().Be("customName");
    }

    [Fact]
    public void IncludeIfNull_CanBeSet()
    {
        var attr = new ResponseErrorPropertyAttribute { IncludeIfNull = true };

        attr.IncludeIfNull.Should().BeTrue();
    }

    [Fact]
    public void Attribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestExceptionWithProperty).GetProperty(nameof(TestExceptionWithProperty.UserId));
        var attr = property?.GetCustomAttributes(typeof(ResponseErrorPropertyAttribute), false)
            .FirstOrDefault() as ResponseErrorPropertyAttribute;

        attr.Should().NotBeNull();
    }

    private class TestExceptionWithProperty : Exception
    {
        [ResponseErrorProperty]
        public string? UserId { get; set; }
    }
}
